using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.Core
{
	public abstract class User : UserBase
	{
		public User(User user)
			: base(user)
		{
		}

		protected User(Server server, Locations location, HostMask mask, Authorizations level)
			: base(server, location, mask, level)
		{
		}

		public override void SetModes(UserBase user, string modes)
		{
			string[] tok = modes.Split(new char[] { ' ' }, 2);
			SetModes(user, tok[0], tok.Length > 1 ? tok[1] : string.Empty);
		}

		public override void SetModes(UserBase user, string flags, string arguments)
		{
			string[] tok = arguments.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			char[] chars = flags.ToCharArray();
			bool add = true;
			int arg = 0;

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
					if (mode == 0 || !Server.Domains.UserModeAttrs.ContainsKey(mode))
					{
						if (user != null)
						{
							user.SendNumeric(Numerics.ERR_UMODEUNKNOWNFLAG, chars[i], ":That not a valid mode character.");
						}
						break;
					}

					string argument;
					bool valid = true;
					bool missingArgs = false;
					UserModeAttribute attr = Server.Domains.UserModeAttrs[mode];
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
						if (user != null)
						{
							user.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":Mode", chars[i], " is programmed incorrectly.  Please file a bug report.");
						}
						continue;
					}
					else if (missingArgs)
					{
						if (user != null)
						{
							user.SendNumeric(Numerics.ERR_NEEDMOREPARAMS, "MODE", ":Expected parameter for channelmode ", chars[i] + ".");
						}
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

			Server.Commit(this);
			SendCommand("MODE", user, user.Mask.Nickname, string.Join(" ", bargs));
		}

		private bool CheckMode(UserBase user, UserModeAttribute attr, string argument)
		{
			if (user == null)
			{
				return true;
			}

			switch (attr.Level)
			{
			case Authorizations.Unregistered:
				if (user.Level < Authorizations.Unregistered)
				{
					user.SendNumeric(Numerics.ERR_NOTREGISTERED, "MODE", ":You aren't fully connected at present.");
					return false;
				}
				break;

			case Authorizations.Registered:
				if (user.Level < Authorizations.Registered)
				{
					user.SendNumeric(Numerics.ERR_NOLOGIN, "MODE", ":You must be identified with services to do that.");
					return false;
				}
				break;

			case Authorizations.NetworkOperator:
				if (user.Level < Authorizations.NetworkOperator)
				{
					user.SendNumeric(Numerics.ERR_UNIQOPRIVSNEEDED, "MODE", ":You must be a network operator to do that.");
					return false;
				}
				break;

			case Authorizations.Service:
				if (user.Level < Authorizations.Service)
				{
					user.SendNumeric(Numerics.ERR_UNIQOPRIVSNEEDED, "MODE", ":That is for use only.");
					return false;
				}
				break;

			default:
				return false;
			}

			return true;
		}

		private bool AddMode(UserBase user, UserModeAttribute attr, string argument)
		{
			try
			{
				ReceivedUserModeEventArgs e = new ReceivedUserModeEventArgs(Server, this, user, argument);
				Server.Domains.AddUserModeHandlers[attr.Mode].Invoke(this, e);

				if (e.IsCanceled)
				{
					return false;
				}
				else
				{
					Modes[attr.Mode] = argument ?? string.Empty;
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error adding mode {0}: {1}", ex);
				return false;
			}
		}

		private bool RemoveMode(UserBase user, UserModeAttribute attr, string argument)
		{
			try
			{
				ReceivedUserModeEventArgs e = new ReceivedUserModeEventArgs(Server, this, user, argument);
				Server.Domains.RemoveUserModeHandlers[attr.Mode].Invoke(this, e);

				if (e.IsCanceled)
				{
					return false;
				}
				else
				{
					Modes.Remove(attr.Mode);
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error removing mode {0}: {1}", ex);
				return false;
			}
		}

		public override bool ValidateNickname(string nickname)
		{
			if (nickname.Length > Server.Settings.MaximumNicknameLength)
			{
				SendNumeric(Numerics.ERR_ERRONEUSNICKNAME, nickname, ":That nickname is too long.");
				return false;
			}

			if (!Validate(nickname, CoreProtocol.NICKNAME_CHARS))
			{
				SendNumeric(Numerics.ERR_ERRONEUSNICKNAME, nickname, ":That nickname contains invalid character(s).");
				return false;
			}
			else
			{
				return true;
			}
		}

		public override bool IsExcepted(ChannelBase channel)
		{
			for (int m = 0; m < channel.Exceptions.Count; m++)
			{
				if (Mask.Matches(channel.Exceptions[m]))
				{
					return true;
				}
			}
			return false;
		}

		public override void OnConnected()
		{
			Server.UsersById.Add(Id, this);

			base.OnConnected();

			if (Level == Authorizations.NetworkOperator)
			{
				Server.NetworkOperators.Add(this);
			}
		}

		public override bool IsBanned(ChannelBase channel)
		{
			if (IsExcepted(channel))
			{
				return false;
			}

			for (int m = 0; m < channel.Bans.Count; m++)
			{
				if (Mask.Matches(channel.Bans[m]))
				{
					return true;
				}
			}

			return false;
		}

		public override bool IsQuieted(ChannelBase channel)
		{
			if (IsExcepted(channel))
			{
				return false;
			}

			for (int m = 0; m < channel.Quiets.Count; m++)
			{
				if (Mask.Matches(channel.Quiets[m]))
				{
					return true;
				}
			}

			return false;
		}

		public override bool CanSendToChannel(ChannelBase channel, bool skipInChannelCheck, string action)
		{
			if (!skipInChannelCheck && channel.HasFlag("NoExternal") && GetChannel(channel.Name) == null)
			{
				SendNumeric(Numerics.ERR_NOTONCHANNEL, channel.Name, ":You cannot " + action + " while not in that channel.");
				return false;
			}
			else if (channel.HasFlag("Muted") && !IsVoice(channel))
			{
				SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, channel.Name, ":You cannot " + action + " while that channel is muted.");
				return false;
			}
			else if (IsBanned(channel))
			{
				SendNumeric(Numerics.ERR_BANNEDFROMCHAN, channel.Name, ":You cannot " + action + " while banned.");
				return false;
			}
			else if (IsQuieted(channel))
			{
				SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, channel.Name, ":You cannot " + action + " while quieted.");
				return false;
			}
			else
			{
				return true;
			}
		}

		public override string MakeUsername(string username)
		{
			StringBuilder builder = new StringBuilder(username.Length + 1).Append('-');
			char[] chars = username.ToCharArray();
			byte max = Server.Settings.MaximumUsernameLength;
			for (int i = 0; i < chars.Length && i < max; i++)
			{
				if (CoreProtocol.USERNAME_CHARS.Contains(chars[i]))
				{
					builder.Append(chars[i]);
				}
			}

			if (builder.Length == 1)
			{
				builder.Append("Invalid");
			}

			return builder.ToString();
		}

		private bool Validate(string input, char[] valid)
		{
			char[] chars = input.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if (!valid.Contains(chars[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override ChannelBase GetChannel(string name)
		{
			string id = name.ToLower();
			return Channels.ContainsKey(id) ? Channels[id] : null;
		}

		public virtual void Start()
		{
		}

		public bool RequireAccess(Authorizations level, string action = "do that")
		{
			if (Level < level)
			{
				switch (Level)
				{
				case Authorizations.Disconnected:
				case Authorizations.Quitting:
				case Authorizations.Banned:
					Dispose("Below-normal authorization: " + Enum.GetName(typeof(Authorizations), Level));
					return false;

				case Authorizations.Connecting:
					SendNumeric(Numerics.ERR_NOTREGISTERED, ":You need to fully connect before you can " + action + ".");
					return false;

				case Authorizations.Unidentified:
					SendNumeric(Numerics.ERR_NOLOGIN, ":You are using a registered nickname.  You need to log in before you can use normal commands.  If this is not your account, change your nickname using /nick.");
					return false;
				}

				switch (level)
				{
				case Authorizations.Registered:
					SendNumeric(Numerics.ERR_NOLOGIN, ":Only users identified with services can " + action + ".");
					return false;

				case Authorizations.NetworkOperator:
					SendNumeric(Numerics.ERR_NOPRIVILEGES, ":Only network operators can " + ".");
					return false;

				default:
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		public override string CompileNumeric(ushort numeric, params object[] args)
		{
			return string.Format(":{0} {1:000} {2} {3}", Server.Id, numeric, Mask.Nickname, string.Join(" ", args));
		}

		public override string CompileCommand(string command, params object[] args)
		{
			List<object> full = new List<object>(new object[]
			{
				command,
			});
			full.AddRange(args);

			return string.Format(Server.Domains.UserCommandAttrs[command].Format, full.ToArray());
		}

		public override string CompileCommand(string command, string origin, params object[] args)
		{
			List<object> full = new List<object>(new object[]
			{
				command,
				origin,
			});
			full.AddRange(args);

			return string.Format(Server.Domains.UserCommandAttrs[command].Format, full.ToArray());
		}

		public override void WriteLine(string line)
		{
		}

		public override void BroadcastInclusive(string command, params object[] args)
		{
			string line = CompileCommand(command, args);
			List<UserBase> users = new List<UserBase>();

			foreach (ChannelBase c in Channels.Values)
			{
				if (c.HasFlag("Arena") && !IsVoice(c))
				{
					continue;
				}

				foreach (UserBase u in c.Users.Values)
				{
					if (users.Contains(u))
					{
						continue;
					}
					users.Add(u);

					u.WriteLine(line);
				}
			}
		}

		public override void BroadcastExclusive(string command, params object[] args)
		{
			string line = CompileCommand(command, args);
			List<UserBase> users = new List<UserBase>();

			foreach (ChannelBase c in Channels.Values)
			{
				if (c.HasFlag("Arena") && !IsVoice(c))
				{
					continue;
				}

				foreach (UserBase u in c.Users.Values)
				{
					if (u == this || users.Contains(u))
					{
						continue;
					}
					users.Add(u);

					u.WriteLine(line);
				}
			}
		}

		public override bool HasFlag(string flag)
		{
			lock (Flags)
			{
				return Flags.Contains(flag);
			}
		}

		public override bool SetFlag(string flag)
		{
			if (HasFlag(flag))
			{
				return false;
			}

			lock (Flags)
			{
				Flags.Add(flag);
			}

			Server.Commit(this);
			return true;
		}

		public override bool ClearFlag(string flag)
		{
			bool retval;
			lock (Flags)
			{
				retval = Flags.Remove(flag);
			}

			Server.Commit(this);
			return retval;
		}

		public override bool HasSessionFlag(string flag)
		{
			lock (SessionFlags)
			{
				return SessionFlags.Contains(flag);
			}
		}

		public override bool SetSessionFlag(string flag)
		{
			if (HasSessionFlag(flag))
			{
				return false;
			}

			lock (SessionFlags)
			{
				SessionFlags.Add(flag);
			}
			return true;
		}

		public override bool ClearSessionFlag(string flag)
		{
			lock (SessionFlags)
			{
				return SessionFlags.Remove(flag);
			}
		}

		public override string PrefixHostMask(ChannelBase channel)
		{
			string prefix = channel.GetPrefix(this);
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return Mask.ToString();
			}
			else if (HasFlag("MultiPrefix"))
			{
				return prefix + Mask;
			}
			else
			{
				return prefix[0].ToString() + Mask;
			}
		}

		public override string PrefixNickname(ChannelBase channel)
		{
			string prefix = channel.GetPrefix(this);
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return Mask.Nickname;
			}
			else if (HasFlag("MultiPrefix"))
			{
				return prefix + Mask.Nickname;
			}
			else
			{
				return prefix[0] + Mask.Nickname;
			}
		}

		public override void Names(string name)
		{
			ChannelBase channel = GetChannel(name);
			if (channel == null)
			{
				SendNumeric(Numerics.ERR_NOTONCHANNEL, name, ":You are not in that channel.");
				return;
			}
			Names(channel);
		}

		public override void Names(ChannelBase channel)
		{
			string[] start = new string[] { channel.Name };
			List<string> args = new List<string>(start);

			foreach (UserBase u in channel.Users.Values)
			{
				if (args.Count >= 50 + start.Length)
				{
					SendNumeric(Numerics.RPL_NAMREPLY, args.ToArray());
					args = new List<string>(start);
				}

				if (!channel.HasFlag("Arena") || u.IsVoice(channel))
				{
					args.Add((args.Count == start.Length ? ":" : "") + u.PrefixNickname(channel));
				}
			}

			if (args.Count > start.Length)
			{
				SendNumeric(Numerics.RPL_NAMREPLY, args.ToArray());
			}
			SendNumeric(Numerics.RPL_ENDOFNAMES, channel.Name, "End of NAMES list.");
		}

		public override void ErrorNeedMoreParams(string command)
		{
			SendNumeric(Numerics.ERR_NEEDMOREPARAMS, command, ":That command requires more parameters.");
		}

		public override void SendNumeric(Numerics numeric, params object[] message)
		{
			SendNumeric((ushort)numeric, message);
		}

		public override void SendNumeric(ushort numeric, params object[] args)
		{
			WriteLine(CompileNumeric(numeric, args));
		}

		public override bool AddToChannel(ChannelBase channel)
		{
			string id = channel.Name.ToLower();
			if (Channels.ContainsKey(id))
			{
				return false;
			}

			try
			{
				channel.Users[Id] = this;
				Channels[id] = channel;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public override void Dispose(string reason)
		{
			Properties["LastSeenTime"] = DateTime.Now;

			if (Server.IsRunning)
			{
				if (Level == Authorizations.NetworkOperator)
				{
					Server.NetworkOperators.Remove(this);
				}

				Server.Commit(this);

				foreach (ChannelBase c in Channels.Values)
				{
					c.RemoveUser(this, false);
				}

				Server.UsersByMask.Remove(Mask);
				Server.UsersById.Remove(Id);
				if (Level >= Authorizations.Registered)
				{
					List<UserBase> users = Server.UsersByAccount[Mask.Account];
					if (users.Count < 2)
					{
						Server.UsersByAccount.Remove(Mask.Account);
					}
					else
					{
						users.Remove(this);
					}
				}
			}

			base.Dispose(reason);
		}

		public override void ErrorAlreadyRegistered(string command)
		{
			SendNumeric(Numerics.ERR_ALREADYREGISTERED, command, ":You have already passed connection negotiation stage.  (If you see this message repeatedly, there is likely a bug in your client.)");
		}

		public override bool IsOwner(ChannelSurrogate channel)
		{
			if (channel.Name[0] == '.' && Level >= Authorizations.NetworkOperator)
			{
				return true;
			}

			string prefix;
			if (channel.Prefixes.ContainsKey(Mask.Account))
			{
				prefix = channel.Prefixes[Mask.Account];
				if (string.IsNullOrEmpty(prefix))
				{
					return false;
				}
			}
			else
			{
				return false;
			}

			if (string.IsNullOrEmpty(prefix))
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

		public override bool IsProtect(ChannelBase channel)
		{
			string prefix = channel.GetPrefix(this);
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

		public override bool IsOperator(ChannelBase channel)
		{
			string prefix = channel.GetPrefix(this);
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

		public override bool IsHalfOperator(ChannelBase channel)
		{
			string prefix = channel.GetPrefix(this);
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

		public override bool IsVoice(ChannelBase channel)
		{
			string prefix = channel.GetPrefix(this);
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

		public override void SendCommand(string command, params object[] args)
		{
			WriteLine(CompileCommand(command, args));
		}

		public override void SendCommand(string command, string origin, params object[] args)
		{
			WriteLine(CompileCommand(command, origin, args));
		}

		public override void Commit()
		{
			Server.Commit(this);

			base.Commit();
		}
	}
}