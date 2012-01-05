﻿using System.Collections.Generic;
using System.Threading;
using Aselia.Common;
using Aselia.Common.Core;

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

		public override ChannelBase GetChannel(string name)
		{
			name = name.ToLower();
			return Channels.ContainsKey(name) ? Channels[name] : null;
		}

		public virtual void Start()
		{
		}

		public bool RequireAccess(Authorizations level, string action = "do that")
		{
			if (Level < level)
			{
				switch (level)
				{
				case Authorizations.Disconnected:
				case Authorizations.Quitting:
				case Authorizations.Banned:
					Dispose();
					break;

				case Authorizations.Connecting:
					SendNumeric(Numerics.ERR_NOTREGISTERED, "You need to fully connect you can" + action + ".");
					break;
				}

				switch (level)
				{
				case Authorizations.Registered:
					SendNumeric(Numerics.ERR_NOLOGIN, "Only users identified with services can " + action + ".");
					break;

				case Authorizations.NetworkOperator:
					SendNumeric(Numerics.ERR_NOPRIVILEGES, "Only network operators can " + ".");
					break;
				}
				return false;
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
				Server.Id,
				command,
				Mask.Nickname,
			});
			full.AddRange(args);

			return string.Format(Server.Domains.UserCommandAttrs[command].Format, full.ToArray());
		}

		public override void WriteLine(string line)
		{
		}

		public void BroadcastInclusive(string command, params object[] args)
		{
			string line = CompileCommand(command, args);

			foreach (ChannelBase c in Channels.Values)
			{
				foreach (UserBase u in c.Users)
				{
					u.WriteLine(line);
				}
			}
		}

		public void BroadcastExclusive(string command, params string[] args)
		{
			string line = CompileCommand(command, args);

			foreach (ChannelBase c in Channels.Values)
			{
				foreach (UserBase u in c.Users)
				{
					if (u == this)
					{
						continue;
					}
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
			return true;
		}

		public override bool ClearFlag(string flag)
		{
			lock (Flags)
			{
				return Flags.Remove(flag);
			}
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
			string prefix = channel.Prefixes[Mask];
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
			string prefix = channel.Prefixes[Mask];
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
			string[] start = new string[] { Mask.Nickname, " = ", channel.Name };
			List<string> args = new List<string>(start);

			foreach (User u in channel.Users)
			{
				if (args.Count >= 50 + start.Length)
				{
					SendNumeric(Numerics.RPL_NAMREPLY, args.ToArray());
					args = new List<string>(start);
				}

				if (!channel.HasFlag("Arena") || u.IsVoice(channel))
				{
					args.Add(u.PrefixNickname(channel));
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

		public bool AddToChannel(ChannelBase channel, string prefix = "")
		{
			try
			{
				if (!channel.Prefixes.TryAdd(Mask, prefix))
				{
					return false;
				}
				lock (channel.Users)
				{
					channel.Users.Add(this);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public override void Dispose()
		{
			if (Level > Authorizations.Connecting)
			{
				BroadcastInclusive("QUIT", "Client disposed.");
			}

			foreach (ChannelBase c in Channels.Values)
			{
				lock (c.Users)
				{
					c.Users.Remove(this);
				}
			}

			for (int i = 0; i < 100; i++)
			{
				UserBase dump;
				if (Server.Users.TryRemove(Mask, out dump))
				{
					break;
				}
				Thread.Sleep(1);
			}
		}

		public override void ErrorAlreadyRegistered(string command)
		{
			SendNumeric(Numerics.ERR_ALREADYREGISTERED, command, ":You have already passed connection negotiation stage.  (If you see this message repeatedly, there is likely a bug in your client.)");
		}

		public override bool IsOwner(ChannelBase channel)
		{
			string prefix = channel.Prefixes[Mask];
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

		public override bool IsProtect(ChannelBase channel)
		{
			string prefix = channel.Prefixes[Mask];
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
			string prefix = channel.Prefixes[Mask];
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
			string prefix = channel.Prefixes[Mask];
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
			string prefix = channel.Prefixes[Mask];
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
	}
}