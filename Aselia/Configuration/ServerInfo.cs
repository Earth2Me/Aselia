using System;
using System.Net;

namespace Aselia.Configuration
{
	[Serializable]
	public class ServerInfo
	{
		public string Id { get; set; }

		public IPEndPoint RemoteEndPoint { get; set; }
	}
}