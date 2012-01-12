using System;

namespace Aselia.Common.Core.Configuration
{
	[Serializable]
	public class Binding : MarshalByRefObject
	{
		public ushort Port { get; set; }

		public Protocols Protocol { get; set; }

		public byte Backlog { get; set; }
	}
}