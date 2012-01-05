using System;
using Aselia.Common.Core;
using Aselia.Common.Hotswap;

namespace Aselia
{
	[Serializable]
	public class Server : ServerBase
	{
		public Server()
		{
		}

		public Server(DomainManager domains)
			: base(domains, Environment.MachineName)
		{
		}

		public Server(DomainManager domains, Server server)
			: base(domains, server)
		{
		}

		public void Dispose()
		{
		}
	}
}