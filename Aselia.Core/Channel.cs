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
	[Serializable]
	public class Channel : ChannelBase
	{
		public override bool IsGlobal
		{
			get { return Name[0] != Protocol.CP_LOCAL; }
		}

		public override bool IsSystem
		{
			get { return Name[0] == Protocol.CP_SYSTEM; }
		}

		public override bool IsRegistered
		{
			get { return Name[0] == Protocol.CP_REGISTERED; }
		}

		public Channel()
			: base()
		{
		}

		public Channel(Channel clone)
			: base(clone)
		{
		}

		public Channel(Server server, string name, string id)
			: base(server, name, id)
		{
		}

		public override string GetModeString()
		{
			StringBuilder builder = new StringBuilder("+");

			foreach (Modes m in Modes.Keys)
			{
				builder.Append(m.ToChar());
			}

			foreach (string a in Modes.Values)
			{
				builder.Append(' ').Append(a);
			}

			return builder.ToString();
		}

		public override unsafe void AddPrefix(UserBase user, char add)
		{
			if (user.Level >= Authorizations.Service)
			{
				return;
			}
			if (add == '$' || add == '!')
			{
				return;
			}
			if (IsSystem && user.Level >= Authorizations.NetworkOperator)
			{
				return;
			}

			if (Prefixes.ContainsKey(user.Mask.Account))
			{
				char[] chars = Prefixes[user.Mask.Account].ToCharArray();

				int max = chars.Length + 1;
				int len = 0;

				char* prefix = stackalloc char[max];

				for (int i = 0; i < Protocol.RANK_CHARS.Length && len < max; i++)
				{
					char c = Protocol.RANK_CHARS[i];
					if (add == c || chars.Contains(c))
					{
						prefix[len++] = c;
					}
				}

				Prefixes[user.Mask.Account] = new string(prefix, 0, len);
			}
			else
			{
				Prefixes[user.Mask.Account] = add.ToString();
			}

			Server.Commit(this);
		}

		public override void Dispose()
		{
			if (Server.IsRunning)
			{
				Server.Channels.Remove(Id);
				Commit();
			}

			base.Dispose();
		}

		public override void RemoveUser(UserBase user, bool removeFromUser = true)
		{
			Users.Remove(user.Id);

			if (removeFromUser)
			{
				user.Channels.Remove(Id);
			}

			if (Users.Count < 1)
			{
				Dispose();
			}
		}

		public override void RemovePrefix(UserBase user, char c)
		{
			if (Prefixes.ContainsKey(user.Mask.Account))
			{
				string value = Prefixes[user.Mask.Account].Replace(c.ToString(), string.Empty);
				if (value == string.Empty)
				{
					Prefixes.Remove(user.Mask.Account);
				}
				else
				{
					Prefixes[user.Mask.Account] = value;
				}

				Server.Commit(this);
			}
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
					if (mode == 0 || !Server.Domains.ChannelModeAttrs.ContainsKey(mode))
					{
						if (user != null)
						{
							user.SendNumeric(Numerics.ERR_UNKNOWNMODE, chars[i], ":That not a valid mode character.");
						}
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
			Broadcast("MODE", user, Name, string.Join(" ", bargs));
		}

		private bool CheckMode(UserBase user, ChannelModeAttribute attr, string argument)
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

			if (user.Level >= Authorizations.Service)
			{
				return true;
			}

			if (!CheckRank(user, attr))
			{
				user.SendNumeric(Numerics.ERR_CHANOPPRIVSNEEDED, "MODE", ":You need to be a higher rank in the channel to set that mode.");
				return false;
			}
			else
			{
				return true;
			}
		}

		private bool CheckRank(UserBase user, ChannelModeAttribute attr)
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

		private bool RemoveMode(UserBase user, ChannelModeAttribute attr, string argument)
		{
			try
			{
				ReceivedChannelModeEventArgs e = new ReceivedChannelModeEventArgs(Server, this, user, argument);
				Server.Domains.RemoveChannelModeHandlers[attr.Mode].Invoke(this, e);

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

		public override UserBase GetUser(string nick, UserBase notifyOnError = null)
		{
			string id = nick.ToLower();
			if (Users.ContainsKey(id))
			{
				return Users[id];
			}

			if (notifyOnError != null)
			{
				notifyOnError.SendNumeric(Numerics.ERR_USERNOTINCHANNEL, nick, Name, ":That user is not on the channel.");
			}
			return null;
		}

		private bool AddModePrefix(UserBase source, char prefix, User target)
		{
			if (target == null)
			{
				source.SendNumeric(Numerics.ERR_USERNOTINCHANNEL, "*", ":That user is not in " + Name + ".");
				return false;
			}
			else
			{
				AddPrefix(target, prefix);
				return true;
			}
		}

		private bool RemoveModePrefix(UserBase source, char prefix, User target)
		{
			if (target == null)
			{
				source.SendNumeric(Numerics.ERR_USERNOTINCHANNEL, "MODE", ":That user is not in " + Name + ".");
				return false;
			}
			else
			{
				RemovePrefix(target, prefix);
				return true;
			}
		}

		private bool AddMode(UserBase user, ChannelModeAttribute attr, string argument)
		{
			try
			{
				ReceivedChannelModeEventArgs e = new ReceivedChannelModeEventArgs(Server, this, user, argument);
				Server.Domains.AddChannelModeHandlers[attr.Mode].Invoke(this, e);

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

		public override string GetPrefix(UserBase user)
		{
			if (user.Level >= Authorizations.Service)
			{
				return "$";
			}
			else if (IsSystem && user.Level >= Authorizations.NetworkOperator)
			{
				return "~";
			}
			else if (Prefixes.ContainsKey(user.Mask.Account))
			{
				return Prefixes[user.Mask.Account];
			}
			else if (user.Level == Authorizations.NetworkOperator)
			{
				return "!";
			}
			else
			{
				return string.Empty;
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
			lock (Flags)
			{
				if (!Flags.Contains(flag))
				{
					Flags.Add(flag);
					Server.Commit(this);
					return true;
				}
				else
				{
					return false;
				}
			}
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

		public override void SetModes(UserBase user, string modes)
		{
			string[] tok = modes.Split(new char[] { ' ' }, 2);
			SetModes(user, tok[0], tok.Length > 1 ? tok[1] : string.Empty);
		}

		public override void Broadcast(string command, UserBase sender, params object[] arguments)
		{
			List<object> full = new List<object>(new object[]
				{
					sender == null ? Server.Id : (object)sender.Mask,
				});
			full.AddRange(arguments);
			object[] args = full.ToArray();

			if (sender == null || !HasFlag("Arena") || sender.IsVoice(this))
			{
				foreach (UserBase u in Users.Values)
				{
					u.SendCommand(command, args);
				}
			}
			else
			{
				sender.SendCommand(command, args);
			}
		}

		public override void Commit()
		{
			Server.Commit(this);

			base.Commit();
		}
	}
}