using System;

namespace Aselia
{
	[Serializable]
	public class HostMask : MarshalByRefObject
	{
		public const string NICKNAME_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789[]{}-_|`";
		public static readonly char[] NICKNAME_CHARS = NICKNAME_STRING.ToCharArray();

		private static char[] FIRST_SPLIT = new char[] { '@' };
		private static char[] SECOND_SPLIT = new char[] { '!' };

		private Match NickMatch = Match.Null;
		private Match UserMatch = Match.Null;
		private Match HostMatch = Match.Null;

		public string Nickname { get; set; }

		public string Username { get; set; }

		public string Hostname { get; set; }

		public string Account { get; set; }

		public static HostMask Parse(string raw)
		{
			HostMask mask = new HostMask();

			if (raw.StartsWith("$a:") && raw.Length > 3)
			{
				mask.Account = raw.Substring(3);
			}
			else
			{
				string[] tok = raw.Split(FIRST_SPLIT, 2);
				if (tok.Length == 2)
				{
					tok = raw.Split(SECOND_SPLIT, 2);
				}
				else
				{
					mask.Hostname = tok[1];
					mask.HostMatch = GetMatchValue(tok[1]);

					tok = tok[0].Split(SECOND_SPLIT, 2);
				}
				if (tok.Length == 2)
				{
					mask.Nickname = tok[0];
					mask.NickMatch = GetMatchValue(tok[0]);
					mask.Username = tok[1];
					mask.UserMatch = GetMatchValue(tok[1]);
				}
				else
				{
					mask.Username = tok[0];
					mask.UserMatch = GetMatchValue(tok[0]);
				}
			}

			return mask;
		}

		private static Match GetMatchValue(string p)
		{
			return p == "*" ? Match.All : p.Contains("*") || p.Contains("?") ? Match.Glob : Match.Identical;
		}

		public bool Matches(HostMask wildcard)
		{
			if (wildcard.Account != null)
			{
				return wildcard.Account == Account;
			}
			else
			{
				return IsMatch(Nickname, wildcard.NickMatch, wildcard.Nickname)
					&& IsMatch(Username, wildcard.UserMatch, wildcard.Username)
					&& IsMatch(Hostname, wildcard.HostMatch, wildcard.Hostname);
			}
		}

		private static bool IsMatch(string subject, Match match, string wildcard)
		{
			switch (match)
			{
			case Match.Glob:
				if (!subject.GlobIsMatch(wildcard))
				{
					return false;
				}
				break;

			case Match.Identical:
				if (subject != wildcard)
				{
					return false;
				}
				break;
			}
			return true;
		}

		public override string ToString()
		{
			if (Account != null && Nickname == null)
			{
				return "$a:" + Account;
			}

			string retval = null;

			if (Nickname != null)
			{
				retval = Nickname;
			}

			if (Username != null)
			{
				if (retval == null)
				{
					retval = Username;
				}
				else
				{
					retval += "!" + Username;
				}
			}

			if (Hostname != null)
			{
				if (retval == null)
				{
					retval = Hostname;
				}
				else
				{
					retval += "@" + Hostname;
				}
			}

			return retval;
		}

		private enum Match
		{
			Null,
			All,
			Identical,
			Glob,
		}
	}
}