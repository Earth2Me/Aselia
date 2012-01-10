using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Aselia.Common.Core
{
	[Serializable]
	public abstract class UserBase : UserSurrogate, IDisposable
	{
		public Locations Location { get; protected set; }

		public string Id { get; set; }

		public string Gecos { get; set; }

		public ConcurrentDictionary<string, ChannelBase> Channels { get; protected set; }

		public ServerBase Server { get; protected set; }

		protected List<string> SessionFlags { get; set; }

		public abstract void BroadcastInclusive(string command, params object[] args);

		public abstract void BroadcastExclusive(string command, params object[] args);

		public abstract ChannelBase GetChannel(string name);

		public abstract void WriteLine(string line);

		public abstract string CompileCommand(string command, params object[] args);

		public abstract string CompileCommand(string command, string origin, params object[] args);

		public abstract string CompileNumeric(ushort numeric, params object[] args);

		public abstract void SendNumeric(ushort numeric, params object[] args);

		public abstract void SendCommand(string command, params object[] args);

		public abstract void SendCommand(string command, string origin, params object[] args);

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

		public abstract bool AddToChannel(ChannelBase channel);

		public abstract void SendNumeric(Numerics numeric, params object[] message);

		public abstract void ErrorAlreadyRegistered(string command);

		public abstract bool ValidateNickname(string nickname);

		public abstract string MakeUsername(string username);

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
			Gecos = clone.Gecos;
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
			Dispose("User disposed.");
		}

		public virtual void Dispose(string reason)
		{
			Server.Commit(this);
		}

		public virtual void OnConnected()
		{
		}

		public virtual void ReplyISupport()
		{
		}

		public virtual void OnPong()
		{
		}

		public virtual void OnPing()
		{
		}

		public virtual void OnMaskChanged()
		{
		}
	}
}