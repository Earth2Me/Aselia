using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Core.Configuration;
using Aselia.Common.Hotswap;
using Aselia.Common.Security;
using Aselia.Core;
using Aselia.Core.Configuration;
using Aselia.Core.Security;

namespace Aselia
{
	public class Server : ServerBase
	{
		private readonly List<ListenerInfo> Listeners = new List<ListenerInfo>();
		private readonly LineSet Lines;
		private readonly Timer SaveTimer;
		private ServerInfo Info;
		private ServerInfo[] RemoteInfo;
		private Dictionary<string, RemoteServer> Remotes;
		private Dictionary<string, RemoteServer> DirectRemotes;
		private bool Odd;
		private bool Unloaded;

		public bool NetworkEstablished { get; private set; }

		public override CertificateManagerBase Certificates { get; set; }

		public override Version CoreVersion { get; set; }

		public override string CoreName { get; set; }

		public new Cache Cache
		{
			get { return base.Cache as Cache; }
			set { base.Cache = value; }
		}

		public Server(DomainManager domains)
			: base(domains, Environment.MachineName)
		{
			SaveTimer = new Timer(SaveProc);
			Lines = new LineSet();
			DirectRemotes = new Dictionary<string, RemoteServer>();
			Remotes = new Dictionary<string, RemoteServer>();
			Settings = InitializeSettings();
			LocalRehash();

			Cache = Cache.Load();
			if (Cache == null)
			{
				Cache = Cache.Create();
				if (!Cache.Save())
				{
					Console.WriteLine("Unable to save cache.  Will not start until cache is writable for security.");
					Environment.Exit(1);
					return;
				}
			}

			Certificates = new CertificateManager();
			string password = Settings.CertificatePassword;
			if (!Certificates.Load(Id, password) && !Certificates.Generate(Id, password))
			{
				Console.WriteLine("Unable to generate certificate on non-Windows OS.  There must be a single, valid X.509 certificate file named 'Certificate.{0}.*' in the current directory.", Id);
				Environment.Exit(1);
				return;
			}

			Initialize();
		}

		public Server(DomainManager domains, ServerBase cloneBase)
			: base(domains, cloneBase)
		{
			if (cloneBase is Server)
			{
				Server clone = (Server)cloneBase;

				Cache = clone.Cache;
				DirectRemotes = clone.DirectRemotes;
				RemoteInfo = clone.RemoteInfo;
				Remotes = clone.Remotes;
				Lines = clone.Lines;
				NetworkEstablished = clone.NetworkEstablished;
				Listeners = clone.Listeners;

				if (clone.SaveTimer.Change(Timeout.Infinite, Timeout.Infinite))
				{
					clone.SaveTimer.Dispose();
				}
			}

			SaveTimer = new Timer(SaveProc);

			Initialize();
		}

		private void Initialize()
		{
			CoreName = Protocol.CORE_NAME;
			CoreVersion = new Version(Protocol.CORE_VERSION);

			int save = Settings.CacheCommitInterval;
			SaveTimer.Change(save, save);
		}

		public override void LocalRehash()
		{
			Settings.Load(new FileInfo("Settings.xml"));
		}

		public override void GlobalRehash()
		{
			LocalRehash();
			// TODO: Rehash remote servers.
		}

		public override bool LogIn(UserBase user, string account, byte[] password)
		{
			try
			{
				if (!Cache.Accounts.ContainsKey(account))
				{
					return false;
				}
				UserSurrogate cache = Cache.Accounts[account];

				if (password.Length != cache.Password.Length)
				{
					return false;
				}
				for (int i = 0; i < password.Length; i++)
				{
					if (password[i] != cache.Password[i])
					{
						return false;
					}
				}

				cache.LastSeen = DateTime.Now;
				user.Load(cache);
				user.Mask.Account = account;

				if (UsersByAccount.ContainsKey(account))
				{
					UsersByAccount[account].Add(user);
				}
				else
				{
					UsersByAccount.Add(account, new List<UserBase>(new UserBase[] { user }));
				}

				user.SetModes(null, "+r" + (user.Level < Authorizations.NetworkOperator ? "" : "o"), string.Empty);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public override bool Register(UserBase user, byte[] password, string email)
		{
			try
			{
				if (user.Level >= Authorizations.Registered)
				{
					return false;
				}

				user.Password = password;
				user.Properties["E-Mail"] = email;
				user.Level = Cache.Accounts.Count > 0 ? Authorizations.Registered : Authorizations.NetworkOperator;
				user.Mask.Account = user.Id;
				Cache.Accounts.Add(user.Mask.Account, user);
				user.Commit();

				UsersByAccount.Add(user.Mask.Account, new List<UserBase>(new UserBase[] { user }));

				user.SendNumeric(Numerics.RPL_REGISTERED, ":You are now registered and logged in.  Use /login password to log in next time you connect.");
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void SaveProc(object state)
		{
			if (!CommitCache())
			{
				Console.WriteLine("Unable to save cache database.  Trying again in 30 seconds.");
				SaveTimer.Change(30000, 30000);
			}
		}

		public override bool CommitCache()
		{
			foreach (KeyValuePair<string, ChannelBase> kv in Channels)
			{
				Cache.Channels[kv.Key] = new ChannelSurrogate(kv.Value);
			}

			foreach (KeyValuePair<string, List<UserBase>> kv in UsersByAccount)
			{
				if (kv.Value.Count > 0 && !string.IsNullOrEmpty(kv.Key) && kv.Key[0] != '/')
				{
					Cache.Accounts[kv.Key] = new UserSurrogate(kv.Value[0]);
				}
			}

			return Cache.Save();
		}

		public override void Commit(ChannelBase channel)
		{
			if (!channel.IsRegistered && !channel.IsSystem)
			{
				return;
			}

			Cache.Channels[channel.Id] = new ChannelSurrogate(channel);
		}

		public override void Commit(UserBase user)
		{
			if (string.IsNullOrEmpty(user.Mask.Account))
			{
				return;
			}

			foreach (UserBase u in UsersByMask.Values)
			{
				if (u.Mask.Account == user.Mask.Account)
				{
					u.Flags = user.Flags;
					u.Password = user.Password;
					u.Properties = user.Properties;
				}
			}

			if (user.Mask.Account[0] == '/')
			{
				return;
			}

			Cache.Accounts[user.Mask.Account] = new UserSurrogate(user);
		}

		public override UserBase GetUser(string nickname)
		{
			string id = nickname.ToLower();

			if (!UsersById.ContainsKey(id))
			{
				return null;
			}

			UserBase user = UsersById[id];
			if (user.Level < Authorizations.Unregistered)
			{
				return null;
			}
			else
			{
				return user;
			}
		}

		public override ChannelBase CreateChannel(string name, UserBase user)
		{
			Channel channel = new Channel(this, name, name.ToLower());

			if (Cache.Channels.ContainsKey(channel.Id))
			{
				channel.Load(Cache.Channels[channel.Id]);
			}
			else
			{
				if (channel.IsSystem && user.Level < Authorizations.NetworkOperator)
				{
					user.SendNumeric(Numerics.ERR_UNIQOPRIVSNEEDED, ":You need to be a network operator to register a system channel.");
					return null;
				}

				if (channel.IsRegistered && user.Level < Authorizations.Registered)
				{
					user.SendNumeric(Numerics.ERR_NOLOGIN, ":You need to be registered to own a channel.  To create a temporary channel, prefix the channel name with # instead of !.");
					return null;
				}

				channel.AddPrefix(user, '~');

				switch (channel.Name[0])
				{
				case '!':
					channel.SetModes(null, Settings.DefaultChannelModesReg);
					break;

				case '#':
					channel.SetModes(null, Settings.DefaultChannelModesTemp);
					break;

				case '&':
					channel.SetModes(null, Settings.DefaultChannelModesLoc);
					break;
				}
			}

			if (channel != null)
			{
				Channels.Add(channel.Id, channel);
			}

			return channel;
		}

		public override bool IsValidChannel(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}

			if (name.Length < 2)
			{
				return false;
			}

			char[] chars = name.ToCharArray();

			if (!Protocol.CHANNEL_PREFIX_CHARS.Contains(chars[0]))
			{
				return false;
			}

			if (chars.Length > Settings.MaximumChannelLength)
			{
				return false;
			}

			for (int i = 1; i < name.Length; i++)
			{
				if (!Protocol.CHANNEL_CHARS.Contains(chars[i]))
				{
					return false;
				}
			}

			return true;
		}

		public override ChannelBase GetChannel(string name)
		{
			name = name.ToLower();
			return Channels.ContainsKey(name) ? Channels[name] : null;
		}

		private void Bind(ListenerInfo info, bool rebind)
		{
			if (Unloaded)
			{
				return;
			}

			if (rebind)
			{
				Console.WriteLine("Appear to have lost a binding.  Rebinding.");
			}
			info.Listener.Start(info.Binding.Backlog);
			info.Listener.BeginAcceptTcpClient(OnBeginAcceptTcpClient, info);
		}

		private void OnBeginAcceptTcpClient(IAsyncResult ar)
		{
			if (Unloaded)
			{
				return;
			}

			ListenerInfo info = (ListenerInfo)ar.AsyncState;
			try
			{
				if (!info.Listener.Server.IsBound)
				{
					Bind(info, true);
					return;
				}
				if (Unloaded) // Check a second time because there's some delay.
				{
					return;
				}
				TcpClient client = info.Listener.EndAcceptTcpClient(ar);

				try
				{
					switch (info.Binding.Protocol)
					{
					case Protocols.Rfc2812:
					case Protocols.Rfc2812Ssl:
						Console.WriteLine("Client connecting from {0}.", client.Client.RemoteEndPoint);
						AcceptClient(client, info);
						break;

					case Protocols.InterServer:
						Console.WriteLine("Client connecting from {0}.", client.Client.RemoteEndPoint);
						AcceptServer(client, info);
						break;

					default:
						client.Close();
						break;
					}
				}
				catch
				{
					client.Close();
				}

				info.Listener.BeginAcceptTcpClient(OnBeginAcceptTcpClient, info);
			}
			catch
			{
				Bind(info, true);
			}
		}

		private void AcceptServer(TcpClient client, ListenerInfo info)
		{
			// TODO
		}

		private void AcceptClient(TcpClient client, ListenerInfo info)
		{
			IPEndPoint ep = (IPEndPoint)client.Client.RemoteEndPoint;
			if (IsKLined(ep.Address))
			{
				Console.WriteLine("Client is K:lined!  Dropping.");
				client.Close();
				return;
			}

			string ip = ep.Address.ToString();
			HostMask mask = HostMask.Parse("*!:" + ep.Port + "@" + ip);
			mask.Account = "/" + ip;
			LocalUser user = new LocalUser(this, client, mask, info.Binding.Protocol == Protocols.Rfc2812 ? false : true);
			user.Start();

			UsersByMask.Add(user.Mask, user);
		}

		public override bool IsQLined(string nickname)
		{
			Regex[] qs = Lines.Q;
			for (int i = 0; i < qs.Length; i++)
			{
				if (qs[i] == null)
				{
					continue;
				}
				if (qs[i].IsMatch(nickname))
				{
					return true;
				}
			}
			return false;
		}

		public override unsafe bool IsKLined(IPAddress fullIp)
		{
			byte[] bytes = fullIp.GetAddressBytes();
			if (bytes.Length < 4)
			{
				return true;
			}

			unchecked
			{
				uint ip;
				fixed (byte* pBytes = bytes)
				{
					int offset = bytes.Length - 4;
					ip = *(uint*)&pBytes[offset];
				}

				Cidr[] ks = Lines.K;
				for (int i = 0; i < ks.Length; i++)
				{
					Cidr cidr = ks[i];
					uint mask = cidr.Ip >> (32 - cidr.Mask);
					uint match = ip >> (32 - cidr.Mask);
					if (match == mask)
					{
						return true;
					}
				}
			}

			return false;
		}

		public void OnJoinedLate()
		{
			NetworkEstablished = true;
			ConnectRemotes(!Odd);
		}

		private void ConnectRemotes(bool toggle)
		{
			for (int i = toggle ? 0 : 1; i < RemoteInfo.Length; i += 2)
			{
				try
				{
					Remotes.Add(RemoteInfo[i].Id, new RemoteServer(this, RemoteInfo[i]));
				}
				catch
				{
				}
			}
		}

		public override void Restart()
		{
			Stop();

			Remotes.Clear();
			ConnectRemotes(Odd);

			try
			{
				foreach (Binding b in Settings.Bindings)
				{
					for (int i = 0; i < b.Interfaces.Length; i++)
					{
						try
						{
							IPAddress ip;
							if (!IPAddress.TryParse(b.Interfaces[i], out ip))
							{
								Console.WriteLine("Skipping invalid interface: {0}", b.Interfaces[i]);
								continue;
							}

							ListenerInfo listener = new ListenerInfo(new TcpListener(ip, b.Port), b);
							Bind(listener, false);
							Listeners.Add(listener);
						}
						catch (Exception ex)
						{
							Console.WriteLine("Error binding to interface: {0}", ex.Message);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to read bindings from settings.  Cannot receive incoming connections!  ({0})", ex.Message);
			}
		}

		public override void Unload()
		{
			Unloaded = true;

			foreach (ListenerInfo l in Listeners)
			{
				try
				{
					l.Listener.Stop();
				}
				catch
				{
				}
			}
		}

		public override void Load()
		{
			foreach (ListenerInfo l in Listeners)
			{
				try
				{
					l.Listener.Start();
					l.Listener.BeginAcceptTcpClient(OnBeginAcceptTcpClient, Info);
				}
				catch
				{
				}
			}
		}

		public override void Stop()
		{
			SaveTimer.Change(Timeout.Infinite, Timeout.Infinite);

			foreach (ListenerInfo l in Listeners)
			{
				try
				{
					Console.WriteLine("Stopping listener on {0}.", l.Listener.LocalEndpoint);
					l.Listener.Stop();
				}
				catch
				{
				}
			}
			Listeners.Clear();

			foreach (RemoteServer r in Remotes.Values)
			{
				try
				{
					r.SendReloading();
				}
				catch
				{
				}
			}
		}

		public override SettingsBase InitializeSettings()
		{
			Settings settings = new Settings();
			settings.Modified += Settings_Modified;
			return settings;
		}

		private void Settings_Modified(object sender, EventArgs e)
		{
			Console.WriteLine("Settings were modified.");

			SettingsBase settings = (SettingsBase)sender;

			Lines.Load(settings);

			NetworkName = settings.NetworkName;

			bool running = IsRunning;
			if (running)
			{
				Stop();
			}

			Info = null;
			List<ServerInfo> others = new List<ServerInfo>();
			foreach (ServerInfo i in settings.NetworkServers)
			{
				if (Info == null)
				{
					Odd = !Odd;
				}

				if (i.Id == Id)
				{
					Info = i;
				}
				else
				{
					others.Add(i);
				}
			}

			if (Info == null)
			{
				Console.WriteLine("Could not find {0} in list of servers.  Shutting down.", Id);
				Environment.Exit(2);
				return;
			}

			RemoteInfo = others.ToArray();

			if (running)
			{
				Restart();
			}
		}

		public override UserSurrogate GetRegisteredUser(string account)
		{
			if (UsersByAccount.ContainsKey(account))
			{
				List<UserBase> users = UsersByAccount[account];
				if (users.Count > 0)
				{
					return UsersByAccount[account][0];
				}
			}

			if (Cache.Accounts.ContainsKey(account))
			{
				return Cache.Accounts[account];
			}
			else
			{
				return null;
			}
		}

		public override ChannelSurrogate GetRegisteredChannel(string channel)
		{
			if (Channels.ContainsKey(channel))
			{
				return Channels[channel];
			}
			else if (Cache.Channels.ContainsKey(channel))
			{
				return Cache.Channels[channel];
			}
			else
			{
				return null;
			}
		}

		public override void Dispose()
		{
			IsRunning = false;

			Stop();

			foreach (ChannelBase c in Channels.Values)
			{
				try
				{
					c.Dispose();
				}
				catch
				{
				}
			}

			foreach (UserBase u in UsersByMask.Values)
			{
				try
				{
					u.Dispose("Server shutting down.");
				}
				catch
				{
				}
			}

			CommitCache();
		}

		private class ListenerInfo
		{
			public TcpListener Listener { get; private set; }

			public Binding Binding { get; private set; }

			public ListenerInfo(TcpListener listener, Binding binding)
			{
				Listener = listener;
				Binding = binding;
			}
		}

		private class LineSet
		{
			public Cidr[] K { get; set; }

			public Regex[] Q { get; set; }

			public LineSet()
			{
			}

			public void Load(SettingsBase settings)
			{
				try
				{
					KLine[] ks = settings.KLines.ToArray();
					K = new Cidr[ks.Length];
					for (int i = 0; i < K.Length; i++)
					{
						K[i] = ks[i].Ban;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to load K:lines: {0}", ex.Message);
				}

				try
				{
					QLine[] qs = settings.QLines.ToArray();
					Q = new Regex[qs.Length];
					for (int i = 0; i < Q.Length; i++)
					{
						try
						{
							Q[i] = new Regex(qs[i].Ban, RegexOptions.Compiled | RegexOptions.IgnoreCase);
						}
						catch (Exception ex)
						{
							Console.WriteLine("The following Q:line is invalid: {0} ({1})", qs[i], ex.Message);
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to load Q:lines: {0}", ex.Message);
				}
			}
		}
	}
}