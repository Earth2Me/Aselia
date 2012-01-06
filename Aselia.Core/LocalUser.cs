using System;
using System.Net.Sockets;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.Core
{
	public sealed class LocalUser : User
	{
		private readonly TcpClient Client;
		private readonly SafeStream Stream;
		private readonly new Server Server;
		private readonly bool Encrypted;

		public LocalUser(LocalUser clone)
			: base(clone)
		{
			Client = clone.Client;
			Stream = clone.Stream;
			Server = clone.Server;
			Encrypted = clone.Encrypted;
			Initialize();
		}

		public LocalUser(Server server, TcpClient client, HostMask mask, bool encrypted)
			: base(server, Locations.Local, mask, Authorizations.Connecting)
		{
			Server = server;
			Client = client;
			Stream = new SafeStream(client.GetStream());
			Encrypted = encrypted;
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
			BeginRead();
			SendCommand("NOTICE", Server.Id, "*", "*** Looking up your hostname...");
			SendCommand("NOTICE", Server.Id, "*", "*** Found your hostname");
		}

		private void BeginRead()
		{
			Stream.BeginReadLine(OnBeginReadLine, null);
		}

		public override void ReplyVersion()
		{
			SendNumeric(Numerics.RPL_VERSION,
				"CHANTYPES=" + Protocol.CHANNEL_PREFIX_STRING,
				"EXCEPTS",
				"INVEX",
				"CHANMODES=eIbq,k,flj,CFLOPQcgimnpstuz",
				"CHANLIMIT=" + Protocol.CHANNEL_PREFIX_STRING + ":" + Server.Settings["MaximumChannels"],
				"PREFIX=(XOaohvx)$~&@%+!",
				"MAXLIST=bqeI:" + Server.Settings["MaximumListSize"],
				"MODES=4",
				"NETWORK=" + Server.Settings["NetworkName"],
				"KNOCK",
				"STATUSMSG=$~&@%+!",
				"are supported by this server");
			SendNumeric(Numerics.RPL_VERSION,
				"SAFELIST",
				"CASEMAPPING=rfc1459",
				"CHARSET=asscii",
				"NICKLEN=" + Server.Settings["MaximumNicknameLength"],
				"CHANNELLEN=" + Server.Settings["MaximumChannelLength"],
				"TOPICLEN=" + Server.Settings["MaximumTopicLength"],
				"CLIENTVER=3.0",
				"TARGMAX=NAMES:1,LIST:1,KICK:1,WHOIS:1,PRIVMSG:1,NOTICE:1",
				"EXTBAN=$,a",
				"are supported by this server");

			base.ReplyVersion();
		}

		public override void OnConnected()
		{
			Level = Authorizations.Normal;

			SendNumeric(Numerics.RPL_WELCOME, string.Format(":Welcome to the {0} Internet Relay Chat Network {1}", Server.Settings["NetworkName"], Mask.Nickname));
			SendNumeric(Numerics.RPL_YOURHOST, string.Format(":Your host is {0}[{1}], running version {2}={3}", Environment.MachineName, Client.Client.LocalEndPoint, Server.CoreName, Server.CoreVersion));
			SendNumeric(Numerics.RPL_CREATED, string.Format(":This server was created at {0}", Server.Created));
			SendNumeric(Numerics.RPL_MYINFO, string.Format(":{0} {1}={2} ABCDEFGHIJKKLMNOPQRSTUVWXYZabcdefghijkklmnopqrstuvwxyz0123456789 ABCDEFGHIJKKLMNOPQRSTUVWXYZabcdefghijkklmnopqrstuvwxyz0123456789 bkloveqjfIOaxhX", Environment.MachineName, Server.CoreName, Server.CoreVersion));
			ReplyVersion();

			base.OnConnected();
		}

		private void OnBeginReadLine(IAsyncResult ar)
		{
			string line = Stream.EndReadLine(ar);
			try
			{
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
				Stream.BeginReadLine(OnBeginReadLine, null);
			}
		}

		public override void WriteLine(string line)
		{
			Stream.BeginWriteLine(line);

			base.WriteLine(line);
		}

		public override void Dispose()
		{
			base.Dispose();

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