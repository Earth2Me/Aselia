using System;
using System.Collections.Generic;
using Aselia.Common.Flags;

namespace Aselia.Common.Core
{
	[Serializable]
	public class ChannelSurrogate : MarshalByRefObject
	{
		public string Id { get; set; }

		public IDictionary<Modes, string> Modes { get; set; }

		public IDictionary<string, string> Prefixes { get; set; }

		public IDictionary<string, object> Properties { get; set; }

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
			Load(clone);
		}

		protected ChannelSurrogate(string name, string id)
		{
			Id = id;
			Modes = new Dictionary<Modes, string>();
			Properties = new Dictionary<string, object>();
			Prefixes = new Dictionary<string, string>();
			Bans = new List<HostMask>();
			InviteExcepts = new List<HostMask>();
			Exceptions = new List<HostMask>();
			Quiets = new List<HostMask>();
			Flags = new List<string>();
			Name = name;
		}

		public virtual void Load(ChannelSurrogate clone)
		{
			Id = clone.Id;
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

		public virtual void Commit()
		{
		}
	}
}