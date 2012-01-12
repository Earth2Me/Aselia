using System;
using System.Net;

namespace Aselia.Common.Core.Configuration
{
	[Serializable]
	public class Cidr : MarshalByRefObject
	{
		public uint Ip { get; set; }

		public byte Mask { get; set; }

		public Cidr()
		{
		}

		public Cidr(uint ip, byte bits)
		{
			Ip = ip;
			Mask = bits;
		}

		public unsafe Cidr(IPAddress ip, byte bits)
		{
			Mask = bits;

			unchecked
			{
				byte[] bytes = ip.GetAddressBytes();
				fixed (byte* pBytes = bytes)
				{
					int offset = bytes.Length - 4;
					Ip = *(uint*)&pBytes[offset];
				}
			}
		}

		public unsafe Cidr(string ip, byte bits)
			: this(IPAddress.Parse(ip), bits)
		{
		}
	}
}