using System;
using System.Net;

namespace Aselia.Configuration
{
	[Serializable]
	public class ServerInfo : MarshalByRefObject
	{
		public string Id { get; set; }

		public IPEndPoint RemoteEndPoint { get; set; }
	}
}