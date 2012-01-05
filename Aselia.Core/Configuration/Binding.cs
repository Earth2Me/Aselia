using System;

namespace Aselia.Core.Configuration
{
	[Serializable]
	public class Binding : MarshalByRefObject
	{
		public bool Encrypted { get; set; }

		public string Address { get; set; }

		public ushort Port { get; set; }

		public Protocols Protocol { get; set; }

		public byte Backlog { get; set; }
	}
}