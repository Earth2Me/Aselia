namespace Aselia.Common.Flags
{
	public static class ModesExtension
	{
		public static Modes ToMode(this char c)
		{
			if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
			{
				return (Modes)c;
			}
			else
			{
				return 0;
			}
		}

		public static char ToChar(this Modes mode)
		{
			return (char)mode;
		}
	}
}