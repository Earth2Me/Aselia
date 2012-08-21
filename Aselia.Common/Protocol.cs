using System;

namespace Aselia.Common
{
	public static class Protocol
	{
		public static readonly char CHANNEL_PREFIX_REGISTERED = '!';
		public static readonly char CHANNEL_PREFIX_TEMPORARY = '#';
		public static readonly char CHANNEL_PREFIX_SYSTEM = '.';
		public static readonly char CHANNEL_PREFIX_LOCAL = '&';
		public static readonly char[] CHANNEL_PREFIX_CHARS = new char[] { CHANNEL_PREFIX_REGISTERED, CHANNEL_PREFIX_TEMPORARY, CHANNEL_PREFIX_SYSTEM, CHANNEL_PREFIX_LOCAL };
		public static readonly string CHANNEL_PREFIX_STRING = new string(CHANNEL_PREFIX_CHARS);
	}
}

