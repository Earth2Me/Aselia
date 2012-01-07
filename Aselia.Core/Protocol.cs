using System.Reflection;

namespace Aselia.Core
{
	public static class Protocol
	{
		public static readonly string CORE_NAME;
		public static readonly string CORE_VERSION = "1.0.0.0";

		public static readonly char CP_REGISTERED = '!';
		public static readonly char CP_TEMPORARY = '#';
		public static readonly char CP_SYSTEM = '.';
		public static readonly char CP_LOCAL = '&';
		public static readonly char[] CHANNEL_PREFIX_CHARS = new char[] { CP_REGISTERED, CP_TEMPORARY, CP_SYSTEM, CP_LOCAL };
		public static readonly string CHANNEL_PREFIX_STRING = new string(CHANNEL_PREFIX_CHARS);
		public static readonly string CHANNEL_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ#.-";
		public static readonly char[] CHANNEL_CHARS = CHANNEL_STRING.ToCharArray();
		public static readonly string RANK_STRING = "$~&@%+!";
		public static readonly char[] RANK_CHARS = RANK_STRING.ToCharArray();
		public static readonly string STATUSMSG = "~&@%+";

		public static readonly string CHANNEL_LIST_MODES = "eIbq";
		public static readonly string CHANNEL_ARG_BOTH_MODES = "k";
		public static readonly string CHANNEL_ARG_SET_MODES = "flj";
		public static readonly string CHANNEL_FLAG_MODES = "CFLMOPQcgimnpstuz";
		public static readonly string CHANNEL_RANK_MODES = "XOaohvx";
		public static readonly string CHANNEL_PARAM_MODES = CHANNEL_ARG_SET_MODES + CHANNEL_RANK_MODES + CHANNEL_LIST_MODES;
		public static readonly string CHANNEL_MODES = CHANNEL_FLAG_MODES + CHANNEL_PARAM_MODES;
		public static readonly string CHANNEL_CATEGORIZED_MODES = string.Join(",", CHANNEL_LIST_MODES, CHANNEL_ARG_BOTH_MODES, CHANNEL_ARG_SET_MODES, CHANNEL_FLAG_MODES);

		public static readonly string USER_MODES = "i";

		public static readonly string NICKNAME_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789[]{}-_|`";
		public static readonly char[] NICKNAME_CHARS = NICKNAME_STRING.ToCharArray();
		public static readonly string USERNAME_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		public static readonly char[] USERNAME_CHARS = USERNAME_STRING.ToCharArray();

		static Protocol()
		{
			Assembly asm = Assembly.GetExecutingAssembly();

			AssemblyTitleAttribute[] name = (AssemblyTitleAttribute[])asm.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
			if (name.Length > 0)
			{
				CORE_NAME = name[0].Title;
			}
			else
			{
				CORE_NAME = "Unknown";
			}
		}
	}
}