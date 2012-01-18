using System;
using System.Text;

namespace Aselia.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			const string ORIGINAL_TEXT = "1231231234";
			string base64 = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(ORIGINAL_TEXT));
			Console.WriteLine(base64);
			Console.ReadKey(true);
		}
	}
}