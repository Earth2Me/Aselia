using System;
using System.Collections.Generic;
using System.Threading;
using Aselia.Modules;

namespace Aselia.Optimized
{
	public abstract class User : MarshalByRefObject, IDisposable
	{
		public bool WaitForCap { get; set; }

		public bool MultiPrefix { get; set; }

		public Authorizations Authorization { get; set; }

		public Locations Location { get; private set; }

		protected Server Server { get; private set; }

		public HostMask Mask { get; set; }

		public string Id { get; set; }

		public List<Channel> Channels { get; private set; }

		protected abstract void Start();

		public abstract void Send(Commands command, string source, params string[] args);

		protected User(Server server, Locations location)
		{
			Authorization = Authorizations.Connecting;
			Server = server;
			Location = location;
			Channels = new List<Channel>();
			Mask = new HostMask()
			{
				Nickname = "*",
				Username = "*",
				Hostname = "*",
			};
		}

		public void SendNumeric(Commands numeric, string text)
		{
			Send(numeric, Server.Id, Mask.Nickname, text);
		}

		public bool RequireAccess(Authorizations level, string action = "do that")
		{
			if (Authorization < level)
			{
				switch (Authorization)
				{
				case Authorizations.Quitting:
				case Authorizations.Banned:
					Dispose();
					break;

				case Authorizations.Connecting:
					Send(Commands.ERR_NOTREGISTERED, Server.Id, Mask.Nickname, "You need to fully connect you can" + action + ".");
					break;
				}

				switch (level)
				{
				case Authorizations.Identified:
					Send(Commands.ERR_NOLOGIN, Server.Id, Mask.Nickname, "Only users identified with services can " + action + ".");
					break;

				case Authorizations.NetworkOperator:
					Send(Commands.ERR_NOPRIVILEGES, Server.Id, Mask.Nickname, "Only network operators can " + ".");
					break;
				}
				return false;
			}
			else
			{
				return true;
			}
		}

		protected void Receive(Commands command, params string[] args)
		{
			try
			{
				if (!Server.Domains.UserCommandHandlers.ContainsKey(command))
				{
					Send(Commands.ERR_UNKNOWNCOMMAND, Server.Id, Mask.Nickname, "That command does not exist on this IRCd.");
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

		public void BroadcastInclusive(Commands command, User source, params string[] args)
		{
			foreach (Channel c in Channels)
			{
				foreach (User u in c.Users)
				{
					u.Send(command, source.PrefixHostMask(c), args);
				}
			}
		}

		public void BroadcastExclusive(Commands command, User source, params string[] args)
		{
			foreach (Channel c in Channels)
			{
				foreach (User u in c.Users)
				{
					if (u == this)
					{
						continue;
					}
					u.Send(command, source.PrefixHostMask(c), args);
				}
			}
		}

		public string PrefixHostMask(Channel channel)
		{
			string prefix = channel.Prefixes[this];
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return Mask.ToString();
			}
			else if (MultiPrefix)
			{
				return prefix + Mask;
			}
			else
			{
				return prefix[0].ToString() + Mask;
			}
		}

		public string PrefixNickname(Channel channel)
		{
			string prefix = channel.Prefixes[this];
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return Mask.Nickname;
			}
			else if (MultiPrefix)
			{
				return prefix + Mask.Nickname;
			}
			else
			{
				return prefix[0] + Mask.Nickname;
			}
		}

		public void Names(Channel channel)
		{
			string[] start = new string[] { Mask.Nickname, " = ", channel.Name };
			List<string> args = new List<string>(start);

			foreach (User u in channel.Users)
			{
				if (args.Count >= 50 + start.Length)
				{
					Send(Commands.RPL_NAMREPLY, Server.Id, args.ToArray());
					args = new List<string>(start);
				}

				if (!channel.Arena || u.IsVoice(channel))
				{
					args.Add(u.PrefixNickname(channel));
				}
			}

			if (args.Count > start.Length)
			{
				Send(Commands.RPL_NAMREPLY, Server.Id, args.ToArray());
			}
			Send(Commands.RPL_ENDOFNAMES, Server.Id, Mask.Nickname, channel.Name, "End of NAMES list.");
		}

		public void Join(Channel channel)
		{
			AddToChannel(channel);
		}

		public void NotImplemented()
		{
			Send(Commands.ERR_UNAVAILRESOURCE, Server.Id, Mask.Nickname, "Support for this feature is pending.");
		}

		public void NeedMoreParams()
		{
			Send(Commands.ERR_NEEDMOREPARAMS, Server.Id, Mask.Nickname, "That command requires more parameters.");
		}

		public bool AddToChannel(Channel channel)
		{
			try
			{
				channel.Prefixes.Add(this, string.Empty);
				channel.Users.Add(this);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public virtual void Dispose()
		{
			if (Authorization > Authorizations.Connecting)
			{
				BroadcastInclusive(Commands.QUIT, this, "Client disposed.");
			}

			foreach (Channel c in Channels)
			{
				for (int i = 0; ; i++)
				{
					try
					{
						c.Users.Remove(this);
						c.Prefixes.Remove(this);
						break;
					}
					catch
					{
						Console.WriteLine("Concurrency issue?");
						if (i == 9)
						{
							Console.WriteLine("Unable to remove user from channel.");
							break;
						}
						else if (i >= 4)
						{
							Thread.Sleep(1);
						}
					}
				}
			}

			try
			{
				Server.Users.Remove(this);
			}
			catch
			{
			}
		}

		public void AlreadyRegistered()
		{
			Send(Commands.ERR_ALREADYREGISTERED, Server.Id, Mask.Nickname, "You have already passed connection negotiation stage.  (If you see this message repeatedly, there is likely a bug in your client.)");
		}

		public bool IsOwner(Channel channel)
		{
			string prefix = channel.Prefixes[this];
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return false;
			}

			switch (prefix[0])
			{
			case '$':
			case '~':
				return true;

			default:
				return false;
			}
		}

		public bool IsProtect(Channel channel)
		{
			string prefix = channel.Prefixes[this];
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return false;
			}

			switch (prefix[0])
			{
			case '$':
			case '~':
			case '&':
				return true;

			default:
				return false;
			}
		}

		public bool IsOperator(Channel channel)
		{
			string prefix = channel.Prefixes[this];
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return false;
			}

			switch (prefix[0])
			{
			case '$':
			case '~':
			case '&':
			case '@':
				return true;

			default:
				return false;
			}
		}

		public bool IsHalfOperator(Channel channel)
		{
			string prefix = channel.Prefixes[this];
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return false;
			}

			switch (prefix[0])
			{
			case '$':
			case '~':
			case '&':
			case '@':
			case '%':
				return true;

			default:
				return false;
			}
		}

		public bool IsVoice(Channel channel)
		{
			string prefix = channel.Prefixes[this];
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return false;
			}

			switch (prefix[0])
			{
			case '$':
			case '~':
			case '&':
			case '@':
			case '%':
			case '+':
				return true;

			default:
				return false;
			}
		}
	}
}