using System;
using System.Collections.Generic;
using System.Text;

namespace Aselia.Core.InterServer
{
	public static class Packer
	{
		public static readonly char[,] PATTERNS =
		{
			{ ' ', 'e', 't', 'a', 'o', 'i', 'n', 's', 'h', 'r', 'd', 'l', 'u', 'c', 'm' },
			{ ' ', 'E', 'T', 'A', 'O', 'I', 'N', 'S', 'H', 'R', 'D', 'L', 'U', 'C', 'M' },
			{ ' ', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '-', '*', '/' },
			{ ' ', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '=', '[', ']' },
			{ ' ', '<', '>', '|', 'P', 'x', '.', 'D', '(', ')', ':', '-', 'S', '/', 'o' },
			{ ' ', 'e', 't', 'a', 'o', 'i', 'n', 's', 'h', 'r', 'd', 'l', 'u', 'c', '*' },
			{ ' ', 'e', 't', 'a', 'o', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
			{ ' ', 'E', 'T', 'A', 'O', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
			{ ' ', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n' },
		};

		public static byte[] Pack(string text)
		{
			char[] chars = text.ToCharArray();
			byte[] best = null;
			List<byte> pack = new List<byte>();
			for (byte p = 0; p < 8; p++)
			{
				unchecked
				{
					pack.Clear();
					bool hi = true;
					byte b = p;
					for (int c = 0; c < chars.Length; c++)
					{
						byte i = 0;
						for (i = 0; i < 0xf; i++)
						{
							if (PATTERNS[p, i] == chars[c])
							{
								break;
							}
						}
						if (i == 0xf)
						{
							byte ascii = (byte)chars[c];
							if (hi)
							{
								b |= (byte)(i << 4);
								pack.Add(b);
								pack.Add(ascii);
								hi = false;
							}
							else
							{
								b = i;
								b |= (byte)(ascii & 0xf0);
								pack.Add(b);
								b = (byte)(ascii & 0xf);
								hi = true;
							}
						}
						else if (hi)
						{
							b |= (byte)(i << 4);
							pack.Add(b);
							hi = false;
						}
						else
						{
							b = i;
							hi = true;
						}
					}

					if (hi)
					{
						b |= 0xf0;
						pack.Add(b);
					}
				}

				if (best == null || pack.Count < best.Length)
				{
					best = pack.ToArray();
				}
			}

			byte[] normal = ASCIIEncoding.ASCII.GetBytes(text);
			if (normal.Length + 1 <= best.Length)
			{
				best = new byte[normal.Length + 1];
				best[0] = 0xff;
				Array.Copy(normal, 0, best, 1, normal.Length);
			}

			return best;
		}

		public static string Unpack(byte[] packed)
		{
			if (packed[0] == 0xff)
			{
				return ASCIIEncoding.ASCII.GetString(packed, 1, packed.Length - 1);
			}

			int pattern = packed[0] & 0xf;
			StringBuilder builder = new StringBuilder();
			for (int b = 0; b < packed.Length; )
			{
				int i = packed[b++] >> 4;
				if (i == 0xf)
				{
					builder.Append((char)packed[b++]);
				}
				else
				{
					builder.Append(PATTERNS[pattern, i]);
				}

				if (b >= packed.Length)
				{
					break;
				}

				i = packed[b] & 0xf;
				if (i == 0xf)
				{
					builder.Append((char)((packed[b++] & 0xf0) | (packed[b] & 0xf)));
				}
				else
				{
					builder.Append(PATTERNS[pattern, i]);
				}
			}

			return builder.ToString();
		}
	}
}