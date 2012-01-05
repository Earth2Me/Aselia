using System;
using System.Linq;

namespace Aselia.Common.Flags
{
	public static class ModesExtension
	{
		public static readonly char[] MODE_CHARS = "ABCDEFGHIJJKLMNOPQRSTUVWXYZabcdefghijjklmnopqrstuvwxyz0123456789".ToCharArray();
		public static readonly Modes[] MODE_FLAGS = Enum.GetValues(typeof(Modes)).Cast<Modes>().ToArray();

		public static Modes ToMode(this char c)
		{
			for (int i = 0; i < MODE_CHARS.Length; i++)
			{
				if (MODE_CHARS[i] == c)
				{
					return MODE_FLAGS[i];
				}
			}
			return 0;
		}

		public static unsafe string ToFlagString(this Modes modes)
		{
			ulong lmodes = (ulong)modes;
			int flagCount = 0;
			for (int i = 0; i < MODE_FLAGS.Length; i++)
			{
				if (unchecked((int)(lmodes >> i) & 1) == 1)
				{
					flagCount++;
				}
			}

			char* mstring = stackalloc char[flagCount];
			for (int i = 0, c = 0; i < MODE_FLAGS.Length; i++)
			{
				if (modes.HasFlag(MODE_FLAGS[i]))
				{
					mstring[c++] = MODE_CHARS[i];
				}
			}

			return new string(mstring, 0, flagCount);
		}
	}
}