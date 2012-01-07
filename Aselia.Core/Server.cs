using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
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

		public override CertificateManagerBase Certificates { get; set; }

		public override Version CoreVersion { get; set; }

		public override string CoreName { get; set; }

		public Server(DomainManager domains)
			: base(domains, Environment.MachineName)
		{
			Lines = new LineSet();
			Settings = LoadSettings();

			Certificates = new CertificateManager();
			string password = (string)Settings.Properties["CertificatePassword"];
			if (!Certificates.Load(Id, password) && !Certificates.Generate(Id, password))
			{
				string error = "There must be a single, valid X.509 certificate file named 'Certificate." + Id + ".*' in the current directory.";
				Console.WriteLine(error);
				throw new Exception(error);
			}

			Initialize();
		}

		public Server(DomainManager domains, Server server)
			: base(domains, server)
		{
			Lines = server.Lines;
			Initialize();
		}

		private void Initialize()
		{
			CoreName = Protocol.CORE_NAME;
			CoreVersion = new Version(Protocol.CORE_VERSION);
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

		public override ChannelBase CreateChannel(string name)
		{
			return new Channel(this, name);
		}

		public override void CommitChannel(ChannelBase channel)
		{
			Channels[channel.Name.ToLower()] = channel;
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

		public override void Restart()
		{
			Stop();

			try
			{
				foreach (Binding b in (List<Binding>)Settings["Bindings"])
				{
					try
					{
						TcpListener listener = new TcpListener(IPAddress.Parse(b.Address), b.Port);
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
		}

		public override SettingsBase LoadSettings()
		{
			Settings settings = new Settings();
			settings.Modified += Settings_Modified;
			settings.Load(new FileInfo("Settings.dat"));
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