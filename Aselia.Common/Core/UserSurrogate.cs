using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Aselia.Common.Flags;

namespace Aselia.Common.Core
{
	[Serializable]
	public class UserSurrogate : MarshalByRefObject
	{
		public HostMask Mask { get; set; }

		public byte[] Password { get; set; }

		public DateTime LastSeen { get; set; }

		public List<Modes> Modes { get; set; }

		public Authorizations Level { get; set; }

		public List<string> Flags { get; set; }

		public ConcurrentDictionary<string, object> Properties { get; set; }

		public UserSurrogate()
		{
		}

		public UserSurrogate(UserSurrogate clone)
		{
			Load(clone);
		}

		protected UserSurrogate(HostMask mask, Authorizations level)
		{
			Mask = mask;
			Level = Level;
			Password = null;
			LastSeen = DateTime.MaxValue;
			Modes = new List<Modes>();
			Properties = new ConcurrentDictionary<string, object>();
			Flags = new List<string>();
		}

		public virtual void Load(UserSurrogate clone)
		{
			Mask = clone.Mask;
			Password = clone.Password;
			LastSeen = clone.LastSeen;
			Modes = clone.Modes;
			Properties = clone.Properties;
			Level = clone.Level;
			Flags = clone.Flags;
		}

		public virtual void Commit()
		{
		}
	}
}