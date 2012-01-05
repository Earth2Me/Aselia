using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Aselia.Common.Core
{
	[Serializable]
	public abstract class UserBase : UserSurrogate, IDisposable
	{
		public Locations Location { get; protected set; }

		public string Id { get; protected set; }

		public ConcurrentDictionary<string, ChannelBase> Channels { get; protected set; }

		public ServerBase Server { get; protected set; }

		protected List<string> SessionFlags { get; protected set; }

		public abstract ChannelBase GetChannel(string name);

		public abstract void WriteLine(string line);

		public abstract string CompileCommand(string command, params object[] args);

		public abstract string CompileNumeric(ushort numeric, params object[] args);

		public abstract void SendNumeric(ushort numeric, params object[] args);

		public abstract void SendCommand(string command, params object[] args);

		public abstract string PrefixHostMask(ChannelBase channel);

		public abstract string PrefixNickname(ChannelBase channel);

		public abstract bool HasFlag(string flag);

		public abstract bool SetFlag(string flag);

		public abstract bool ClearFlag(string flag);

		public abstract bool HasSessionFlag(string flag);

		public abstract bool SetSessionFlag(string flag);

		public abstract bool ClearSessionFlag(string flag);

		public abstract void Names(string name);

		public abstract void Names(ChannelBase channel);

		public abstract bool IsOwner(ChannelBase channel);

		public abstract bool IsProtect(ChannelBase channel);

		public abstract bool IsOperator(ChannelBase channel);

		public abstract bool IsHalfOperator(ChannelBase channel);

		public abstract bool IsVoice(ChannelBase channel);

		public abstract void ErrorNeedMoreParams(string command);

		public abstract bool AddToChannel(ChannelBase channel, string prefix = "");

		public abstract void SendNumeric(Numerics numeric, params object[] message);

		public abstract void ErrorAlreadyRegistered(string command);

		public UserBase()
			: base()
		{
		}

		public UserBase(UserBase clone)
			: base(clone)
		{
			Location = clone.Location;
			Id = clone.Id;
			Channels = clone.Channels;
			Server = clone.Server;
			SessionFlags = clone.SessionFlags;
		}

		protected UserBase(ServerBase server, Locations location, HostMask mask, Authorizations level)
			: base(mask, level)
		{
			Server = server;
			Location = location;
			Channels = new ConcurrentDictionary<string, ChannelBase>();
			SessionFlags = new List<string>();
			if (mask.Nickname != "*" && mask.Nickname != null)
			{
				Id = mask.Nickname.ToLower();
			}
		}

		public virtual void Dispose()
		{
		}
	}
}