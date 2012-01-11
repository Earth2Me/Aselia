using System.Security.Cryptography;
using System.Text;

namespace Aselia.Common
{
	public static class Password
	{
		public static byte[] Hash(string input)
		{
			try
			{
				using (HashAlgorithm hash = SHA512.Create())
				{
					return hash.ComputeHash(ASCIIEncoding.Unicode.GetBytes(input));
				}
			}
			catch
			{
				return null;
			}
		}
	}
}