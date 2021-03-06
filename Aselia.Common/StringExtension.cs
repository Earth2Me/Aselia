﻿using System.Text.RegularExpressions;

namespace Aselia.Common
{
	public static class StringExtension
	{
		public static bool GlobIsMatch(this string input, string glob)
		{
			Regex regex = new Regex("^" + Regex.Escape(glob).Replace(@"\*", ".*").Replace(@"\?", ".") + "$");
			return regex.IsMatch(input);
		}
	}
}