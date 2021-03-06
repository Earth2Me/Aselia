using System;
using System.Net.Sockets;
using System.Threading;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.Core
{
	public sealed class LocalUser : User
	{
		private Timer PingTimer;
		private readonly TcpClient Client;
		private readonly SafeStream Stream;
		private readonly new Server Server;
		private readonly bool Encrypted;
		private volatile bool Ping;
		private bool IsDisposing;
		private bool IsAuthenticated;

		public LocalUser(LocalUser clone)
			: base(clone)
		{
			Client = clone.Client;
			Stream = clone.Stream;
			Server = clone.Server;
			Encrypted = clone.Encrypted;
			Ping = clone.Ping;
			IsAuthenticated = clone.IsAuthenticated;

			if (clone.PingTimer.Change(Timeout.Infinite, Timeout.Infinite))
			{
				clone.PingTimer.Dispose();
			}

			PingTimer = new Timer(PingProc);
			PingTimer.Change(Server.Settings.PingTimeout, Server.Settings.PingTimeout);

			Initialize();
			BeginRead();
		}

		public LocalUser(Server server, TcpClient client, HostMask mask, bool encrypted)
			: base(server, Locations.Local, mask, Authorizations.Connecting)
		{
			Server = server;
			Client = client;
			PingTimer = new Timer(PingProc);
			Encrypted = encrypted;

			if (encrypted)
			{
				IsAuthenticated = false;
				Stream = new SafeStream(client.GetStream(), Server.Certificates.Certificate);
			}
			else
			{
				IsAuthenticated = true;
				Stream = new SafeStream(client.GetStream(), null);
			}

			PingTimer = new Timer(PingProc);
			OnPing();

			Initialize();
		}

		private void Initialize()
		{
			Stream.Disposed += Stream_Disposed;
		}

		private void Stream_Disposed(object sender, EventArgs e)
		{
			Dispose();
		}

		public override bool Register(byte[] password, string email)
		{
			if (Server.Register(this, password, email))
			{
				SetModes(null, "+r" + (Level < Authorizations.NetworkOperator ? "" : "o"), string.Empty);
				return true;
			}
			else
			{
				return false;
			}
		}

		private void Receive(string command, params string[] args)
		{
			try
			{
				if (!Server.Domains.UserCommandHandlers.ContainsKey(command))
				{
					SendNumeric(Numerics.ERR_UNKNOWNCOMMAND, command, ":That command does not exist on this IRCd.");
					return;
				}

				if (!RequireAccess(Server.Domains.UserCommandAttrs[command].Level, "use the " + command + " command"))
				{
					return;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Receive checks for {0} threw an exception: {1}", command, ex);
				SendNumeric(Numerics.ERR_UNAVAILRESOURCE, command, ":Unable to obtain command details.");
				return;
			}

			try
			{
				Server.Domains.UserCommandHandlers[command].Invoke(this, new ReceivedCommandEventArgs(Server, this, command, args));
			}
			catch (Exception ex)
			{
				Console.WriteLine("Command handler for {0} threw an exception: {1}", command, ex);
				SendNumeric(Numerics.ERR_UNAVAILRESOURCE, command, "EXCEPTION", ":" + ex.Message);
			}
		}

		public override void Start()
		{
			Level = Authorizations.Connecting;

			Server.LocalUsers.Add(this);

			BeginRead();
		}

		public override void OnPing()
		{
			PingTimer.Change(Server.Settings.PongTimeout, Timeout.Infinite);
			Ping = true;
		}

		private void PingProc(object state)
		{
			if (Ping)
			{
				Dispose("Ping/pong timeout.");
			}
			else
			{
				SendCommand("PING", Server.Id, Mask, Server.Id);
				OnPing();
			}
		}

		private void BeginRead()
		{
			if (IsAuthenticated)
			{
				Stream.BeginReadLine(OnBeginReadLine, null);
			}
			else
			{
				Stream.BeginAuthenticate(OnBeginAuthenticate);
			}
		}

		private void OnBeginAuthenticate(IAsyncResult ar)
		{
			if (ar.IsCompleted)
			{
				IsAuthenticated = true;
				BeginRead();
			}
		}

		public override void OnPong()
		{
			PingTimer.Change(Server.Settings.PingTimeout, Timeout.Infinite);
			Ping = false;
		}

		public override void ReplyLUsers()
		{
			SendNumeric(Numerics.RPL_LUSERCLIENT, ":There are", Server.UsersById.Count, "users on", Server.Settings.NetworkServers.Length, "servers.");
			SendNumeric(Numerics.RPL_LUSEROP, ":" + Server.NetworkOperators.Count, "IRC operators online.");
			SendNumeric(Numerics.RPL_LUSERCHANNELS, ":" + Server.Channels.Count, "active channels formed.");
			SendNumeric(Numerics.RPL_LUSERCHANNELS, ":" + Server.Cache.Channels.Count, "channels registered.");
			SendNumeric(Numerics.RPL_LUSERME, ":This specific server has", Server.LocalUsers.Count, "users.");

			ReplyUsers();

			base.ReplyLUsers();
		}

		public override void ReplyMotd()
		{
			if (Server.Settings.Motd != null && Server.Settings.Motd.Length > 0)
			{
				SendNumeric(Numerics.RPL_MOTDSTART, ":Message of the Day:");
				for (int i = 0; i < Server.Settings.Motd.Length; i++)
				{
					SendNumeric(Numerics.RPL_MOTD, ":" + Server.Settings.Motd[i]);
				}
				SendNumeric(Numerics.RPL_ENDOFMOTD, ":End of MOTD.");
			}
			else
			{
				SendNumeric(Numerics.ERR_NOMOTD, ":This server does not have a Message of the Day.");
			}

			base.ReplyMotd();
		}

		public override void ReplyUsers()
		{
			base.ReplyUsers();
		}

		public override void ReplyVersion()
		{
			SendNumeric(Numerics.RPL_MYINFO, string.Format("{0} {1}={2} {3} {4} {4}", Server.Id, Server.CoreName, Server.CoreVersion, CoreProtocol.USER_MODES, CoreProtocol.CHANNEL_MODES, CoreProtocol.CHANNEL_PARAM_MODES));
			SendNumeric(Numerics.RPL_ISUPPORT,
				"CHANTYPES=" + Protocol.CHANNEL_PREFIX_STRING,
				"EXCEPTS",
				"INVEX",
				"CHANMODES=" + CoreProtocol.CHANNEL_CATEGORIZED_MODES,
				"CHANLIMIT=" + Protocol.CHANNEL_PREFIX_STRING + ":" + Server.Settings.MaximumChannels,
				"PREFIX=(" + CoreProtocol.CHANNEL_RANK_MODES + ")" + CoreProtocol.RANK_STRING,
				"MAXLIST=" + CoreProtocol.CHANNEL_LIST_MODES + ":" + Server.Settings.MaximumListSize,
				"MODES=10",
				"NETWORK=" + Server.NetworkName,
				"RFC2812",
				// TODO: "IDCHAN=!:5",
				// TODO: "KNOCK",
				// TODO: "STATUSMSG=" + Protocol.STATUSMSG,
				":are supported by this server");
			SendNumeric(Numerics.RPL_ISUPPORT,
				"SAFELIST",
				"CASEMAPPING=ascii",
				"CHARSET=ascii",
				"NICKLEN=" + Server.Settings.MaximumNicknameLength,
				"CHANNELLEN=" + Server.Settings.MaximumChannelLength,
				"TOPICLEN=" + Server.Settings.MaximumTopicLength,
				"TARGMAX=NAMES:1,LIST:1,KICK:1,WHOIS:1,PRIVMSG:1,NOTICE:1,MODE:5",
				"EXTBAN=$,a",
				":are supported by this server");

			base.ReplyVersion();
		}

		public override void OnConnected()
		{
			Level = Server.GetRegisteredUser(Id) == null ? Authorizations.Unregistered : Authorizations.Unidentified;

			SendNumeric(Numerics.RPL_WELCOME, string.Format(":Welcome to the {0} Internet Relay Chat Network {1}", Server.NetworkName, Mask.Nickname));
			SendNumeric(Numerics.RPL_YOURHOST, string.Format(":Your host is {0}[{1}], running version {2}={3}", Server.Id, Client.Client.LocalEndPoint, Server.CoreName, Server.CoreVersion));
			SendNumeric(Numerics.RPL_CREATED, string.Format(":This server was created at {0}", Server.Created));
			ReplyVersion();
			ReplyLUsers();
			ReplyMotd();

			SetModes(null, "+i" + (Encrypted ? "Z" : ""), string.Empty);

			base.OnConnected();

			OnPong();
		}

		private void OnBeginReadLine(IAsyncResult ar)
		{
			try
			{
				OnPong();

				string line = Stream.EndReadLine(ar);
				string[] tok = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

				string cmd = tok[0].ToUpper();

				if (tok.Length < 2)
				{
					Receive(cmd);
				}
				else if (tok[1][0] == ':')
				{
					Receive(cmd, tok[1].Substring(1));
				}
				else
				{
					tok = tok[1].Split(new string[] { " :" }, 2, StringSplitOptions.None);
					if (tok.Length < 2)
					{
						Receive(cmd, tok[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
					}
					else
					{
						string final = tok[1];
						tok = tok[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						string[] args = new string[tok.Length + 1];
						Array.Copy(tok, args, tok.Length);
						args[tok.Length] = final;
						Receive(cmd, args);
					}
				}
			}
			catch
			{
			}
			finally
			{
				try
				{
					Stream.BeginReadLine(OnBeginReadLine, null);
				}
				catch
				{
					Dispose("Socket closed.");
				}
			}
		}

		public override void WriteLine(string line)
		{
			Stream.BeginWriteLine(line);

			base.WriteLine(line);
		}

		public override void Dispose()
		{
			Dispose("Client disposed.");
		}

		public override void Dispose(string reason)
		{
			if (IsDisposing)
			{
				return;
			}
			IsDisposing = true;

			if (PingTimer.Change(Timeout.Infinite, Timeout.Infinite))
			{
				PingTimer.Dispose();
			}

			if (Server.IsRunning)
			{
				if (Level > Authorizations.Connecting)
				{
					BroadcastInclusive("QUIT", Mask.Nickname, reason);
				}
			}

			base.Dispose(reason);

			if (Server.IsRunning)
			{
				Server.LocalUsers.Remove(this);
			}

			try
			{
				if (Client.Connected)
				{
					Client.Close();
				}
			}
			catch
			{
			}
		}
	}
}