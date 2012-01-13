using System;
using System.Collections.Generic;

namespace Aselia.Common.Core
{
	[Serializable]
	public abstract class ChannelBase : ChannelSurrogate, IDisposable
	{
		public ServerBase Server { get; private set; }

		public IDictionary<string, UserBase> Users { get; protected set; }

		public abstract bool IsGlobal { get; }

		public abstract bool IsRegistered { get; }

		public abstract bool IsSystem { get; }

		public abstract string GetModeString();

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
		}

		protected ChannelBase(ServerBase server, string name, string id)
			: base(name, id)
		{
			Server = server;
			Users = new Dictionary<string, UserBase>();
		}

		public abstract void RemoveUser(UserBase user, bool removeFromUser = true);

		public abstract void AddPrefix(UserBase user, char c);

		public abstract void RemovePrefix(UserBase user, char c);

		public abstract void SetModes(UserBase user, string flags, string arguments);

		public abstract void SetModes(UserBase user, string modes);

		public abstract UserBase GetUser(string nick, UserBase notifyOnError = null);

		public abstract string GetPrefix(UserBase user);

		public abstract void Broadcast(string command, UserBase sender, params object[] arguments);

		public virtual void Dispose()
		{
		}
	}
}