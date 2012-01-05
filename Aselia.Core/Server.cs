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
using Aselia.Core;
using Aselia.Core.Configuration;

namespace Aselia
{
	public class Server : ServerBase
	{
		private readonly List<TcpListener> Listeners = new List<TcpListener>();
		private readonly LineSet Lines;

		public Server(DomainManager domains)
			: base(domains, Environment.MachineName)
		{
			Lines = new LineSet();
		}

		public Server(DomainManager domains, Server server)
			: base(domains, server)
		{
			Lines = server.Lines;
		}

		public override UserBase GetUser(string nickname)
		{
			string id = nickname.ToLower();
			foreach (UserBase u in Users.Values)
			{
				if (u.Id == id)
				{
					return u;
				}
			}
			return null;
		}

		public override ChannelBase CreateChannel(string name)
		{
			Channel channel = new Channel(this, name);
			Channels[channel.Name.ToLower()] = channel;
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

		public override void Run()
		{
			Settings = LoadSettings();
			Restart();
		}

		private void OnBeginAcceptTcpClient(IAsyncResult ar)
		{
			if (ar.IsCompleted)
			{
				try
				{
					ListenerInfo info = (ListenerInfo)ar.AsyncState;
					TcpClient client = info.Listener.EndAcceptTcpClient(ar);
					try
					{
						switch (info.Binding.Protocol)
						{
						case Protocols.Traditional:
							AcceptClient(client, info);
							break;

						case Protocols.InterServer:
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
			}
			else
			{
				Console.WriteLine("Appear to have lost a binding.  Rebinding.");
				Restart();
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
				client.Close();
				return;
			}

			string ip = ep.Address.ToString();
			HostMask mask = HostMask.Parse("*!:" + ep.Port + "@" + ip);
			mask.Account = "/" + ip;
			LocalUser user = new LocalUser(this, client, mask, info.Binding.Encrypted);
			user.Start();

			if (!Users.TryAdd(user.Mask, user))
			{
				user.Dispose();
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
			if (bytes.Length < 32)
			{
				return true;
			}

			unchecked
			{
				uint ip;
				fixed (byte* pBytes = bytes)
				{
					int offset = bytes.Length - 32;
					ip = *(uint*)pBytes[offset];
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
			settings.Load(new FileInfo("Settings.xml"));
			return settings;
		}

		private void Settings_Modified(object sender, EventArgs e)
		{
			Lines.Load((SettingsBase)sender);
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
					u.Dispose();
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
					List<KLine> ks = ((List<KLine>)settings.Properties["Q-"]);
					K = new Cidr[ks.Count];
					for (int i = 0; i < Q.Length; i++)
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