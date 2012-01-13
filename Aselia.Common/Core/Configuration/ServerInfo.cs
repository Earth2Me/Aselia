using System;

namespace Aselia.Common.Core.Configuration
{
	[Serializable]
	public class ServerInfo : MarshalByRefObject
	{
		public string Id { get; set; }

		public string InterServerIp { get; set; }

		public ushort InterServerPort { get; set; }
	}
}