using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Aselia.Common.Flags;

namespace Aselia.Common.Core
{
	[Serializable]
	public class ChannelSurrogate : MarshalByRefObject
	{
		public ConcurrentDictionary<Modes, string> Modes { get; set; }

		public ConcurrentDictionary<string, string> Prefixes { get; set; }

		public Dictionary<string, object> Properties { get; set; }

		public List<HostMask> Bans { get; set; }

		public List<HostMask> InviteExcepts { get; set; }

		public List<HostMask> Exceptions { get; set; }

		public List<HostMask> Quiets { get; set; }

		public List<string> Flags { get; set; }

		public string Name { get; set; }

		public ChannelSurrogate()
		{
		}

		public ChannelSurrogate(ChannelSurrogate clone)
		{
			Modes = clone.Modes;
			Prefixes = clone.Prefixes;
			Bans = clone.Bans;
			InviteExcepts = clone.InviteExcepts;
			Exceptions = clone.Exceptions;
			Quiets = clone.Quiets;
			Flags = clone.Flags;
			Name = clone.Name;
			Properties = clone.Properties;
		}

		protected ChannelSurrogate(string name)
		{
			Modes = new ConcurrentDictionary<Modes, string>();
			Properties = new Dictionary<string, object>();
			Prefixes = new ConcurrentDictionary<string, string>();
			Bans = new List<HostMask>();
			InviteExcepts = new List<HostMask>();
			Exceptions = new List<HostMask>();
			Quiets = new List<HostMask>();
			Flags = new List<string>();
			Name = name;
		}
	}
}