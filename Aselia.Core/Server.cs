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
		private readonly List<TcpListener> Listeners = new List<TcpListener>();
		private readonly LineSet Lines;
		private readonly Timer SaveTimer;
		private readonly new Cache Cache;
		private IPAddress[] BindIps;
		private ServerInfo Info;
		private ServerInfo[] RemoteInfo;
		private Dictionary<string, RemoteServer> Remotes;
		private Dictionary<string, RemoteServer> DirectRemotes;
		private bool Odd;

		public bool NetworkEstablished { get; private set; }

		public override CertificateManagerBase Certificates { get; set; }

		public override Version CoreVersion { get; set; }

		public override string CoreName { get; set; }

		public Server(DomainManager domains)
			: base(domains, Environment.MachineName)
		{
			SaveTimer = new Timer(SaveProc);
			Lines = new LineSet();
			DirectRemotes = new Dictionary<string, RemoteServer>();
			Remotes = new Dictionary<string, RemoteServer>();
			Settings = LoadSettings();

			base.Cache = Cache = Cache.Load();
			if (Cache == null)
			{
				base.Cache = Cache = Cache.Create();
				if (!Cache.Save())
				{
					Console.WriteLine("Unable to save cache.  Will not start until cache is writable for security.");
					Environment.Exit(1);
					return;
				}
			}

			Certificates = new CertificateManager();
			string password = (string)Settings.Properties["CertificatePassword"];
			if (!Certificates.Load(Id, password) && !Certificates.Generate(Id, password))
			{
				Console.WriteLine("There must be a single, valid X.509 certificate file named 'Certificate.{0}.*' in the current directory.", Id);
				Environment.Exit(1);
				return;
			}

			Initialize();
		}

		public Server(DomainManager domains, Server clone)
			: base(domains, clone)
		{
			Cache = clone.Cache;
			Certificates = clone.Certificates;
			BindIps = clone.BindIps;
			DirectRemotes = clone.DirectRemotes;
			RemoteInfo = clone.RemoteInfo;
			Remotes = clone.Remotes;
			Lines = clone.Lines;

			SaveTimer = new Timer(SaveProc);
			if (clone.SaveTimer.Change(Timeout.Infinite, Timeout.Infinite))
			{
				clone.Dispose();
			}

			Initialize();
		}

		private void Initialize()
		{
			CoreName = Protocol.CORE_NAME;
			CoreVersion = new Version(Protocol.CORE_VERSION);

			int save = (int)Settings["CacheCommitInterval"];
			SaveTimer.Change(save, save);
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

			foreach (KeyValuePair<HostMask, UserBase> kv in Users)
			{
				if (!string.IsNullOrEmpty(kv.Key.Account) && kv.Key.Account[0] != '/')
				{
					Cache.Accounts[kv.Key.Account] = new UserSurrogate(kv.Value);
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

			foreach (UserBase u in Users.Values)
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
			foreach (UserBase u in Users.Values)
			{
				if (u.Id == id)
				{
					if (u.Level < Authorizations.Normal)
					{
						return null;
					}
					else
					{
						return u;
					}
				}
			}
			return null;
		}

		public override ChannelBase CreateChannel(string name, UserBase user)
		{
			Channel channel = new Channel(this, name, name.ToLower());

			if (Cache.Channels.ContainsKey(channel.Id))
			{
				ChannelSurrogate cache = Cache.Channels[channel.Id];
				channel.Name = cache.Name;
				channel.Modes = cache.Modes;
				channel.Prefixes = cache.Prefixes;
				channel.Quiets = cache.Quiets;
				channel.Bans = cache.Bans;
				channel.Exceptions = cache.Exceptions;
				channel.InviteExcepts = cache.InviteExcepts;
				channel.Properties = cache.Properties;
				channel.Flags = cache.Flags;
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
				channel.SetModes(null, (string)Settings["DefaultChannelModes:" + channel.Name[0]]);
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

			if (chars.Length > (byte)Settings.Properties["MaximumChannelLength"])
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

		private void OnBeginAcceptTcpClient(IAsyncResult ar)
		{
			ListenerInfo info = (ListenerInfo)ar.AsyncState;
			try
			{
				if (!info.Listener.Server.IsBound)
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
			}
			catch
			{
			}
			finally
			{
				try
				{
					info.Listener.BeginAcceptTcpClient(OnBeginAcceptTcpClient, ar.AsyncState);
				}
				catch
				{
					Console.WriteLine("Appear to have lost a binding.  Rebinding.");
					Restart();
				}
			}
		}

		private void AcceptServer(TcpClient client, ListenerInfo info)
		{
			throw new NotImplementedException();
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

			if (!Users.TryAdd(user.Mask, user))
			{
				Console.WriteLine("Error adding user to dictionary.  Concurrency issue?");
				user.Dispose("Concurrency error.");
			}
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
					uint mask = cidr.Ip >> (32 - cidr.Bits);
					uint match = ip >> (32 - cidr.Bits);
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

			if (BindIps.Length < 1)
			{
				return;
			}

			try
			{
				foreach (Binding b in (List<Binding>)Settings["Bindings"])
				{
					for (int i = 0; i < BindIps.Length; i++)
					{
						try
						{
							TcpListener listener = new TcpListener(BindIps[i], b.Port);
							listener.Start(b.Backlog);
							listener.BeginAcceptTcpClient(OnBeginAcceptTcpClient, new ListenerInfo(listener, b));
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
			Stop();
		}

		public override void Load()
		{
			Restart();
		}

		public override void Stop()
		{
			SaveTimer.Change(Timeout.Infinite, Timeout.Infinite);

			foreach (TcpListener l in Listeners)
			{
				try
				{
					l.Stop();
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
					r.Dispose();
				}
				catch
				{
				}
			}
		}

		public override SettingsBase LoadSettings()
		{
			Settings settings = new Settings();
			settings.Modified += Settings_Modified;
			settings.Load(new FileInfo("Settings.db"));
			return settings;
		}

		private void Settings_Modified(object sender, EventArgs e)
		{
			Console.WriteLine("Settings were modified.");

			SettingsBase settings = (SettingsBase)sender;

			Lines.Load(settings);

			PingTimeout = (int)settings["PingTimeout"];
			PongTimeout = (int)settings["PongTimeout"];
			NetworkName = (string)settings["NetworkName"];

			bool running = Running;
			if (running)
			{
				Stop();
			}

			Info = null;
			List<ServerInfo> others = new List<ServerInfo>();
			foreach (ServerInfo i in (List<ServerInfo>)settings["NetworkServers"])
			{
				if (Info == null)
				{
					Odd = !Odd;
				}

				if (i.Id == Id)
				{
					Info = i;

					List<IPAddress> ips = new List<IPAddress>();
					for (int x = 0; x < i.Interfaces.Count; x++)
					{
						IPAddress ip;
						if (IPAddress.TryParse(i.Interfaces[x], out ip))
						{
							ips.Add(ip);
						}
					}
					BindIps = ips.ToArray();

					if (BindIps.Length < 1)
					{
						Console.WriteLine("Warning: Not binding to any interfaces!");
					}
				}
				else
				{
					others.Add(i);
				}
			}

			if (Info == null)
			{
				Console.WriteLine("Could not find {0} in list of servers.  Shutting down.", Id);
				Dispose();
				Environment.Exit(2);
				return;
			}

			RemoteInfo = others.ToArray();

			if (running)
			{
				Restart();
			}
		}

		public override void Dispose()
		{
			Running = false;

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

			foreach (UserBase u in Users.Values)
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
					List<KLine> ks = (List<KLine>)settings.Properties["K-"];
					K = new Cidr[ks.Count];
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
					List<QLine> qs = ((List<QLine>)settings.Properties["Q-"]);
					Q = new Regex[qs.Count];
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