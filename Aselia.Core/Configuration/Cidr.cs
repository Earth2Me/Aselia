using System;
using System.Net;

namespace Aselia.Core.Configuration
{
	[Serializable]
	public class Cidr : MarshalByRefObject
	{
		public uint Ip { get; set; }

		public byte Bits { get; set; }

		public Cidr()
		{
		}

		public Cidr(uint ip, byte bits)
		{
			Ip = ip;
			Bits = bits;
		}

		public unsafe Cidr(IPAddress ip, byte bits)
		{
			Bits = bits;

			unchecked
			{
				byte[] bytes = ip.GetAddressBytes();
				fixed (byte* pBytes = bytes)
				{
					int offset = bytes.Length - 32;
					Ip = *(uint*)pBytes[offset];
				}
			}
		}

		public unsafe Cidr(string ip, byte bits)
			: this(IPAddress.Parse(ip), bits)
		{
		}
	}
}