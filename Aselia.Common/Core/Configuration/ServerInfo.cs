using System;
using System.Collections.Generic;

namespace Aselia.Common.Core.Configuration
{
	[Serializable]
	public class ServerInfo : MarshalByRefObject
	{
		public string Id { get; set; }

		public string InterServerIp { get; set; }

		public List<string> Interfaces { get; set; }

		public ushort InterServerPort { get; set; }
	}
}