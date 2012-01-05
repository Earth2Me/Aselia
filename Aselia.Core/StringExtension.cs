using System.Text.RegularExpressions;

namespace Aselia.Core
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