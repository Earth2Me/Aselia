using System;
using System.Collections.Generic;
using System.Text;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.Optimized
{
	public class Channel : MarshalByRefObject
	{
		public Channel(Server server, string name)
		{
			Server = server;
			Name = name;
			IsGlobal = name[0] == '#' || name[0] == '+';
			Users = new List<User>();
			Prefixes = new Dictionary<User, string>();
		}

		public unsafe void AddPrefix(User user, char c)
		{
			char[] chars = Prefixes[user].ToCharArray();
			int max = chars.Length + 1;
			int len = 0;
			char* prefix = stackalloc char[max];

			for (int i = 0; i < PREFIX_CHARS.Length; i++)
			{
				if (c == PREFIX_CHARS[i] || chars.Contains(PREFIX_CHARS[i]))
				{
					prefix[len++] = c;
				}
			}

			Prefixes[user] = new string(prefix, 0, len);
		}

		public void RemovePrefix(User user, char c)
		{
			Prefixes[user].Replace(c.ToString(), string.Empty);
		}

		public void SetModes(User user, string full)
		{
			string[] tok = full.Split(' ');
			char[] chars = tok[0].ToCharArray();
			bool add = true;
			int arg = 1;

			List<char> AddModes = new List<char>();
			List<string> AddArgs = new List<string>();
			List<char> RemModes = new List<char>();
			List<string> RemArgs = new List<string>();

			for (int i = 0; i < chars.Length; i++)
			{
				switch (chars[i])
				{
				case '+':
					add = true;
					break;

				case '-':
					add = false;
					break;

				default:
					Modes mode = chars[i].ToMode();
					if (mode == 0 || !Server.Domains.ChannelModeAttrs.ContainsKey(mode))
					{
						user.Send(Commands.ERR_UNKNOWNMODE, Server.Id, user.Mask.Nickname, chars[i] + " is not a valid mode character.");
						break;
					}

					string argument;
					bool valid = true;
					bool missingArgs = false;
					ChannelModeAttribute attr = Server.Domains.ChannelModeAttrs[mode];
					switch (attr.Syntax)
					{
					case ModeSyntax.Always:
						if (arg < tok.Length)
						{
							argument = tok[arg++];
						}
						else
						{
							argument = null;
							missingArgs = true;
						}
						break;

					case ModeSyntax.Never:
						argument = null;
						break;

					case ModeSyntax.OnAdd:
						if (add)
						{
							if (arg < tok.Length)
							{
								argument = tok[arg++];
							}
							else
							{
								argument = null;
								missingArgs = true;
							}
						}
						else
						{
							argument = null;
						}
						break;

					default:
						argument = null;
						valid = false;
						break;
					}

					if (!valid)
					{
						user.Send(Commands.ERR_FILEERROR, Server.Id, user.Mask.Nickname, chars[i] + " is programmed incorrectly.  Please file a bug report.");
						continue;
					}
					else if (missingArgs)
					{
						user.Send(Commands.ERR_NEEDMOREPARAMS, Server.Id, user.Mask.Nickname, "Expected parameter for channelmode " + chars[i] + ".");
						continue;
					}

					if (!CheckMode(user, attr, argument))
					{
						break;
					}
					else if (add)
					{
						if (AddMode(user, attr, argument))
						{
							AddModes.Add(chars[i]);
							if (argument != null)
							{
								AddArgs.Add(argument);
							}
						}
					}
					else
					{
						if (RemoveMode(user, attr, argument))
						{
							RemModes.Add(chars[i]);
							if (argument != null)
							{
								RemArgs.Add(argument);
							}
						}
					}
					break;
				}
			}

			if (AddModes.Count + RemModes.Count == 0)
			{
				return;
			}

			StringBuilder modes = new StringBuilder(AddModes.Count + RemModes.Count + 2);
			if (AddModes.Count > 0)
			{
				modes.Append('+').Append(AddModes.ToArray());
			}
			if (RemModes.Count > 0)
			{
				modes.Append('-').Append(RemModes.ToArray());
			}

			string[] bargs = new string[1 + AddArgs.Count + RemArgs.Count];
			bargs[0] = modes.ToString();
			AddArgs.CopyTo(bargs, 1);
			RemArgs.CopyTo(bargs, 1 + AddArgs.Count);

			Broadcast(Commands.MODE, user, bargs);
		}

		private bool CheckMode(User user, ChannelModeAttribute attr, string argument)
		{
			switch (attr.Level)
			{
			case Authorizations.Normal:
				if (user.Authorization < Authorizations.Normal)
				{
					user.Send(Commands.ERR_NOTREGISTERED, Server.Id, user.Mask.Nickname, "You aren't fully connected at present.");
					return false;
				}
				break;

			case Authorizations.Registered:
				if (user.Authorization < Authorizations.Registered)
				{
					user.Send(Commands.ERR_NOLOGIN, Server.Id, user.Mask.Nickname, "You must be identified with services to do that.");
					return false;
				}
				break;

			case Authorizations.NetworkOperator:
				if (user.Authorization < Authorizations.NetworkOperator)
				{
					user.Send(Commands.ERR_UNIQOPRIVSNEEDED, Server.Id, user.Mask.Nickname, "You must be a network operator to do that.");
					return false;
				}
				break;

			case Authorizations.Service:
				if (user.Authorization < Authorizations.Service)
				{
					user.Send(Commands.ERR_UNIQOPRIVSNEEDED, Server.Id, user.Mask.Nickname, "That mode is for internal use only.");
					return false;
				}
				break;

			default:
				user.NotImplemented();
				return false;
			}

			if (user.Authorization >= Authorizations.Service)
			{
				return true;
			}

			if (!CheckRank(user, attr))
			{
				user.Send(Commands.ERR_CHANOPRIVSNEEDED, Server.Id, user.Mask.Nickname, "You need to be a higher rank in the channel to set that mode.");
				return false;
			}
			else
			{
				return true;
			}
		}

		private bool CheckRank(User user, ChannelModeAttribute attr)
		{
			if (attr.Prefix == null)
			{
				return true;
			}

			switch (attr.Prefix.Value)
			{
			case '~':
				return user.IsOwner(this);

			case '&':
				return user.IsProtect(this);

			case '@':
				return user.IsOperator(this);

			case '%':
				return user.IsHalfOperator(this);

			case '+':
				return user.IsVoice(this);

			default:
				return false;
			}
		}

		private bool RemoveMode(User user, ChannelModeAttribute attr, string argument)
		{
			try
			{
				ReceivedChannelModeEventArgs e = new ReceivedChannelModeEventArgs(Server, this, user, argument);
				Server.Domains.RemoveChannelModeHandlers[attr.Mode].Invoke(this, e);
				return !e.IsCanceled;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error removing mode {0}: {1}", ex);
				return false;
			}
		}

		public User GetUser(string nickname, User notifyOnError = null)
		{
			for (int i = 0; i < Users.Count; i++)
			{
				if (Users[i].Mask.Nickname == nickname)
				{
					return Users[i];
				}
			}

			if (notifyOnError != null)
			{
				notifyOnError.Send(Commands.ERR_USERNOTINCHANNEL, Server.Id, notifyOnError.Mask.Nickname, "That user is not on the channel.");
			}
			return null;
		}

		private bool AddModePrefix(User source, char prefix, User target)
		{
			if (target == null)
			{
				source.Send(Commands.ERR_USERNOTINCHANNEL, Server.Id, source.Mask.Nickname, "That user is not in " + Name + ".");
				return false;
			}
			else
			{
				AddPrefix(target, prefix);
				return true;
			}
		}

		private bool RemoveModePrefix(User source, char prefix, User target)
		{
			if (target == null)
			{
				source.Send(Commands.ERR_USERNOTINCHANNEL, Server.Id, source.Mask.Nickname, "That user is not in " + Name + ".");
				return false;
			}
			else
			{
				RemovePrefix(target, prefix);
				return true;
			}
		}

		private bool AddMode(User user, ChannelModeAttribute attr, string argument)
		{
			try
			{
				ReceivedChannelModeEventArgs e = new ReceivedChannelModeEventArgs(Server, this, user, argument);
				Server.Domains.AddChannelModeHandlers[attr.Mode].Invoke(this, e);
				return !e.IsCanceled;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error adding mode {0}: {1}", ex);
				return false;
			}
		}

		public void Broadcast(Commands command, User sender, params string[] arguments)
		{
			if (!Arena || sender.IsVoice(this))
			{
				for (int i = 0; i < Users.Count; i++)
				{
					Users[i].Send(command, sender.PrefixHostMask(this), arguments);
				}
			}
		}
	}
}