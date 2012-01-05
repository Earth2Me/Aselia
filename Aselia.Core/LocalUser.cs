using System;
using System.Net.Sockets;
using System.Text;
using Aselia.Common;

namespace Aselia.Core
{
	public sealed class LocalUser : User
	{
		private readonly TcpClient Client;
		private readonly SafeStream Stream;
		private readonly new Server Server;

		public LocalUser(LocalUser clone)
			: base(clone)
		{
			Client = clone.Client;
			Stream = clone.Stream;
			Server = clone.Server;
		}

		public LocalUser(Server server, TcpClient client)
			: base(server, Locations.Local, HostMask.Parse("*!*@*"), Authorizations.Connecting)
		{
			Server = server;
			Client = client;
			Stream = new SafeStream(client.GetStream());
		}

		private void Receive(string command, params string[] args)
		{
			try
			{
				if (!Server.Domains.UserCommandHandlers.ContainsKey(command))
				{
					SendNumeric(Commands.ERR_UNKNOWNCOMMAND, Server.Id, Mask.Nickname, "That command does not exist on this IRCd.");
					return;
				}

				if (!RequireAccess(Server.Domains.UserCommandLevels[command]))
				{
					return;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Receive checks for {0} threw an exception: {1}", command, ex);
				Send(Commands.ERR_UNAVAILRESOURCE, Server.Id, Mask.Nickname, "Unable to obtain command details.");
				return;
			}

			try
			{
				Server.Domains.UserCommandHandlers[command].Invoke(this, new ReceivedCommandEventArgs(Server, this, command, args));
			}
			catch (Exception ex)
			{
				Console.WriteLine("Command handler for {0} threw an exception: {1}", command, ex);
				Send(Commands.ERR_UNAVAILRESOURCE, Server.Id, Mask.Nickname, "Command handler threw an exception.");
			}
		}

		protected override void Start()
		{
			BeginRead();
		}

		private void BeginRead()
		{
			Stream.BeginReadLine(OnBeginReadLine, null);
		}

		private void OnBeginReadLine(IAsyncResult ar)
		{
			string line = Stream.EndReadLine(ar);
			string[] tok = line.Split(new char[] { ' ' }, 2);

			string cmd = tok[0].ToUpper();
			Commands command;
			if (!Enum.TryParse<Commands>(cmd, false, out command))
			{
				Send(Commands.ERR_UNKNOWNCOMMAND, Server.Id, cmd + " Unknown command");
				return;
			}

			if (tok.Length < 2)
			{
				Receive(command);
			}
			else if (tok[1][0] == ':')
			{
				Receive(command, tok[1].Substring(1));
			}
			else
			{
				tok = tok[1].Split(new string[] { " :" }, 2, StringSplitOptions.None);
				if (tok.Length < 2)
				{
					Receive(command, tok[0].Split(' '));
				}
				else
				{
					string final = tok[1];
					tok = tok[0].Split(' ');
					string[] args = new string[tok.Length + 1];
					Array.Copy(tok, args, tok.Length);
					args[tok.Length] = final;
					Receive(command, args);
				}
			}
		}

		public override void Send(Commands command, string source, params string[] args)
		{
			StringBuilder line = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(source))
			{
				line.Append(':').Append(source).Append(' ');
			}

			line.Append(command.GetToken());

			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length - 1; i++)
				{
					line.Append(' ').Append(args[i]);
				}

				switch (command)
				{
				case Commands.INVITE:
				case Commands.ISON:
				case Commands.JOIN:
				case Commands.KNOCK:
				case Commands.MODE:
				case Commands.NICK:
				case Commands.OPER:
				case Commands.LS:
				case Commands.ACK:
				case Commands.NAMES:
					line.Append(' ');
					break;

				case Commands.KICK:
				case Commands.PART:
					line.Append(args.Length > 2 ? " :" : " ");
					break;

				default:
					line.Append(" :");
					break;
				}
				line.Append(args[args.Length - 1]);
			}

			Stream.WriteLine(line.ToString());
			Stream.Flush();
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