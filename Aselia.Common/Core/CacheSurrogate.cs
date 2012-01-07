using System;
using System.Collections.Generic;

namespace Aselia.Common.Core
{
	[Serializable]
	public class CacheSurrogate : MarshalByRefObject
	{
		public IDictionary<string, UserSurrogate> Accounts { get; set; }

		public IDictionary<string, ChannelSurrogate> Channels { get; set; }

		public CacheSurrogate()
		{
		}

		public CacheSurrogate(CacheSurrogate clone)
		{
			Accounts = clone.Accounts;
			Channels = clone.Channels;
		}
	}
}