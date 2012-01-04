using System;

namespace Aselia.Configuration
{
	[Serializable]
	public class Binding : MarshalByRefObject
	{
		public bool Encrypted { get; set; }

		public long Interface { get; set; }

		public ushort Port { get; set; }

		public Protocols Protocol { get; set; }
	}
}