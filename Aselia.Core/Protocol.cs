namespace Aselia.Core
{
	public static class Protocol
	{
		public static readonly string CHANNEL_PREFIX_STRING = "#+&";
		public static readonly char[] CHANNEL_PREFIX_CHARS = CHANNEL_PREFIX_STRING.ToCharArray();
		public static readonly string CHANNEL_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ#.-";
		public static readonly char[] CHANNEL_CHARS = CHANNEL_STRING.ToCharArray();
		public static readonly string RANK_STRING = "$~&@%+!";
		public static readonly char[] RANK_CHARS = RANK_STRING.ToCharArray();

		public static readonly string NICKNAME_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789[]{}-_|`";
		public static readonly char[] NICKNAME_CHARS = NICKNAME_STRING.ToCharArray();
		public static readonly string USERNAME_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		public static readonly char[] USERNAME_CHARS = USERNAME_STRING.ToCharArray();
	}
}