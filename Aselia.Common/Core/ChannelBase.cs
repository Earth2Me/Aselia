using System;
using System.Collections.Generic;

namespace Aselia.Common.Core
{
	[Serializable]
	public class ChannelBase : ChannelSurrogate, IDisposable
	{
		public ServerBase Server { get; private set; }

		public List<UserBase> Users { get; protected set; }

		protected List<string> Flags { get; protected set; }

		public bool IsGlobal
		{
			get { return Name[0] == '#' || Name[0] == '+'; }
		}

		public bool IsRegistered
		{
			get { return Name[0] == '#'; }
		}

		public abstract bool HasFlag(string flag);

		public abstract bool SetFlag(string flag);

		public abstract bool ClearFlag(string flag);

		public ChannelBase()
			: base()
		{
		}

		public ChannelBase(ChannelBase clone)
			: base(clone)
		{
			Server = clone.Server;
			Users = clone.Users;
			Flags = clone.Flags;
		}

		protected ChannelBase(ServerBase server, string name)
			: base(name)
		{
			Server = server;
			Users = new List<UserBase>();
			Flags = new List<string>();
		}

		public virtual void Dispose()
		{
		}
	}
}