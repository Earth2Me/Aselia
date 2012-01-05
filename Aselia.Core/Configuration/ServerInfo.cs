using System;

namespace Aselia.Core.Configuration
{
	[Serializable]
	public class ServerInfo : MarshalByRefObject
	{
		public string Id { get; set; }

		public string Host { get; set; }

		public ushort Port { get; set; }
	}
}