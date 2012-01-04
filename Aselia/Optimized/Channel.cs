using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aselia.Flags;

namespace Aselia.Optimized
{
	public class Channel
	{
		public const string CHANNEL_CHARS = "#+&";
		public const string PREFIX_STRING = "$~&@%+!";
		public static readonly char[] PREFIX_CHARS = PREFIX_STRING.ToCharArray();

		private readonly Server Server;

		public bool Arena { get; private set; }

		public Dictionary<ChannelModes, object> Modes { get; private set; }

		public Dictionary<User, string> Prefixes { get; private set; }

		public List<HostMask> Bans { get; private set; }

		public List<HostMask> InviteExcepts { get; private set; }

		public List<HostMask> Exceptions { get; private set; }

		public List<HostMask> Quiets { get; private set; }

		public List<User> Users { get; private set; }

		public string Name { get; private set; }

		public bool IsGlobal { get; private set; }

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
					ChannelModes mode = (ChannelModes)chars[i].ToMode();
					if (mode == 0)
					{
						user.Send(Commands.ERR_UNKNOWNMODE, Server.Id, user.Mask.Nickname, chars[i] + " is not a valid mode character.");
						break;
					}

					string argument;
					bool valid = true;
					bool missingArgs = false;
					switch (mode)
					{
					case ChannelModes.Service:
					case ChannelModes.Operator:
					case ChannelModes.Protect:
					case ChannelModes.Owner:
					case ChannelModes.InviteExcept:
					case ChannelModes.Voice:
					case ChannelModes.IrcOp:
					case ChannelModes.Quiet:
					case ChannelModes.HalfOperator:
					case ChannelModes.Exception:
					case ChannelModes.Ban:
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

					case ChannelModes.Arena:
					case ChannelModes.DisableForward:
					case ChannelModes.FreeInvite:
					case ChannelModes.FreeTarget:
					case ChannelModes.InviteOnly:
					case ChannelModes.LargeLists:
					case ChannelModes.LockTopic:
					case ChannelModes.Moderated:
					case ChannelModes.NoActions:
					case ChannelModes.NoColor:
					case ChannelModes.NoCtcps:
					case ChannelModes.NoExternal:
					case ChannelModes.OpModerated:
					case ChannelModes.Permanent:
					case ChannelModes.Private:
					case ChannelModes.RegisteredOnly:
					case ChannelModes.Secret:
						argument = null;
						break;

					case ChannelModes.Forward:
					case ChannelModes.JoinThrottle:
					case ChannelModes.Key:
					case ChannelModes.Limit:
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
						user.Send(Commands.ERR_UNKNOWNMODE, Server.Id, user.Mask.Nickname, chars[i] + " is not a valid channel mode.");
						continue;
					}
					else if (missingArgs)
					{
						user.Send(Commands.ERR_NEEDMOREPARAMS, Server.Id, user.Mask.Nickname, "Expected parameter for channelmode " + chars[i] + ".");
					}

					if (!CheckMode(user, mode, argument))
					{
						break;
					}
					else if (add)
					{
						if (AddMode(user, mode, argument))
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
						if (RemoveMode(user, mode, argument))
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

		private bool CheckMode(User user, ChannelModes mode, string argument)
		{
			switch (mode)
			{
			case ChannelModes.Operator:
			case ChannelModes.Protect:
			case ChannelModes.InviteExcept:
			case ChannelModes.Voice:
			case ChannelModes.Quiet:
			case ChannelModes.HalfOperator:
			case ChannelModes.Exception:
			case ChannelModes.Ban:
			case ChannelModes.Arena:
			case ChannelModes.DisableForward:
			case ChannelModes.FreeInvite:
			case ChannelModes.FreeTarget:
			case ChannelModes.InviteOnly:
			case ChannelModes.LockTopic:
			case ChannelModes.Moderated:
			case ChannelModes.NoActions:
			case ChannelModes.NoColor:
			case ChannelModes.NoCtcps:
			case ChannelModes.NoExternal:
			case ChannelModes.OpModerated:
			case ChannelModes.Private:
			case ChannelModes.Secret:
			case ChannelModes.Forward:
			case ChannelModes.JoinThrottle:
			case ChannelModes.Key:
			case ChannelModes.Limit:
				if (user.Authorization < Authorizations.Normal)
				{
					user.Send(Commands.ERR_NOTREGISTERED, Server.Id, user.Mask.Nickname, "You aren't fully connected at present.");
					return false;
				}
				break;

			case ChannelModes.RegisteredOnly:
				if (user.Authorization < Authorizations.Identified)
				{
					user.Send(Commands.ERR_NOLOGIN, Server.Id, user.Mask.Nickname, "You must be identified with services to do that.");
					return false;
				}
				break;

			case ChannelModes.Permanent:
			case ChannelModes.LargeLists:
				if (user.Authorization < Authorizations.NetworkOperator)
				{
					user.Send(Commands.ERR_UNIQOPRIVSNEEDED, Server.Id, user.Mask.Nickname, "You must be a network operator to do that.");
					return false;
				}
				break;

			case ChannelModes.Service:
			case ChannelModes.Owner:
			case ChannelModes.IrcOp:
				if (user.Authorization < Authorizations.Normal)
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

			if (!CheckRank(user, mode))
			{
				user.Send(Commands.ERR_CHANOPRIVSNEEDED, Server.Id, user.Mask.Nickname, "You need to be a higher rank in the channel to set that mode.");
				return false;
			}
			else
			{
				return true;
			}
		}

		private bool CheckRank(User user, ChannelModes mode)
		{
			switch (mode)
			{
			case ChannelModes.Protect:
				return user.IsOwner(this);

			case ChannelModes.DisableForward:
			case ChannelModes.FreeInvite:
			case ChannelModes.Arena:
			case ChannelModes.InviteExcept:
			case ChannelModes.Operator:
			case ChannelModes.FreeTarget:
			case ChannelModes.NoActions:
			case ChannelModes.NoColor:
			case ChannelModes.NoCtcps:
			case ChannelModes.Forward:
				return user.IsProtect(this);

			case ChannelModes.Ban:
			case ChannelModes.Exception:
			case ChannelModes.HalfOperator:
			case ChannelModes.Moderated:
			case ChannelModes.InviteOnly:
			case ChannelModes.LockTopic:
			case ChannelModes.NoExternal:
			case ChannelModes.OpModerated:
			case ChannelModes.Secret:
			case ChannelModes.Private:
			case ChannelModes.Key:
			case ChannelModes.Limit:
			case ChannelModes.JoinThrottle:
			case ChannelModes.RegisteredOnly:
				return user.IsOperator(this);

			case ChannelModes.Quiet:
			case ChannelModes.Voice:
				return user.IsHalfOperator(this);

			case ChannelModes.LargeLists:
			case ChannelModes.Permanent:
				return true;

			default:
				return false;
			}
		}

		private bool RemoveMode(User user, ChannelModes mode, string argument)
		{
			switch (mode)
			{
			case ChannelModes.Service:
				return RemoveModePrefix(user, '$', GetUser(argument));

			case ChannelModes.Owner:
				return RemoveModePrefix(user, '~', GetUser(argument));

			case ChannelModes.Protect:
				return RemoveModePrefix(user, '&', GetUser(argument));

			case ChannelModes.Operator:
				return RemoveModePrefix(user, '@', GetUser(argument));

			case ChannelModes.HalfOperator:
				return RemoveModePrefix(user, '%', GetUser(argument));

			case ChannelModes.Voice:
				return RemoveModePrefix(user, '+', GetUser(argument));

			case ChannelModes.IrcOp:
				return RemoveModePrefix(user, '!', GetUser(argument));

			case ChannelModes.Quiet:
				return Quiets.Remove(HostMask.Parse(argument));

			case ChannelModes.Ban:
				return Bans.Remove(HostMask.Parse(argument));

			case ChannelModes.InviteExcept:
				return InviteExcepts.Remove(HostMask.Parse(argument));

			case ChannelModes.Exception:
				return Exceptions.Remove(HostMask.Parse(argument));

			case ChannelModes.Arena:
				if (Arena == true)
				{
					Arena = false;
					return true;
				}
				else
				{
					return false;
				}

			default:
				return false;
			}
		}

		public User GetUser(string nickname)
		{
			for (int i = 0; i < Users.Count; i++)
			{
				if (Users[i].Mask.Nickname == nickname)
				{
					return Users[i];
				}
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

		private bool AddMode(User user, ChannelModes mode, string argument)
		{
			HostMask mask;
			switch (mode)
			{
			case ChannelModes.Service:
				return AddModePrefix(user, '$', GetUser(argument));

			case ChannelModes.Operator:
				return AddModePrefix(user, '@', GetUser(argument));

			case ChannelModes.Protect:
				return AddModePrefix(user, '&', GetUser(argument));

			case ChannelModes.Owner:
				return AddModePrefix(user, '~', GetUser(argument));

			case ChannelModes.Voice:
				return AddModePrefix(user, '+', GetUser(argument));

			case ChannelModes.IrcOp:
				return AddModePrefix(user, '!', GetUser(argument));

			case ChannelModes.HalfOperator:
				return AddModePrefix(user, '%', GetUser(argument));

			case ChannelModes.Quiet:
				mask = HostMask.Parse(argument);
				if (!Quiets.Contains(mask))
				{
					Quiets.Add(mask);
					return true;
				}
				return false;

			case ChannelModes.InviteExcept:
				mask = HostMask.Parse(argument);
				if (!InviteExcepts.Contains(mask))
				{
					InviteExcepts.Add(mask);
					return true;
				}
				return false;

			case ChannelModes.Exception:
				mask = HostMask.Parse(argument);
				if (!Exceptions.Contains(mask))
				{
					Exceptions.Add(mask);
					return true;
				}
				return false;

			case ChannelModes.Ban:
				mask = HostMask.Parse(argument);
				if (!Bans.Contains(mask))
				{
					Bans.Add(mask);
					return true;
				}
				return false;

			case ChannelModes.Arena:
				if (!Arena)
				{
					Arena = true;
					return true;
				}
				else
				{
					return false;
				}

			default:
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