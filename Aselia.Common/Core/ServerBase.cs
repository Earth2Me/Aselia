using System;
using System.Collections.Concurrent;
using System.Net;
using Aselia.Common.Core.Configuration;
using Aselia.Common.Hotswap;

namespace Aselia.Common.Core
{
	[Serializable]
	public abstract class ServerBase : MarshalByRefObject, IDisposable
	{
		[NonSerialized]
		private DomainManager _Domains;

		public abstract Version CoreVersion { get; protected set; }

		public abstract string CoreName { get; protected set; }

		public DateTime Created { get; private set; }

		public string Id { get; private set; }

		public bool Running { get; protected set; }

		public ConcurrentDictionary<string, ChannelBase> Channels { get; private set; }

		public ConcurrentDictionary<HostMask, UserBase> Users { get; private set; }

		public SettingsBase Settings { get; set; }

		public abstract ChannelBase GetChannel(string name);

		public abstract bool IsValidChannel(string name);

		public abstract bool IsQLined(string nickname);

		public abstract bool IsKLined(IPAddress fullIp);

		public abstract ChannelBase CreateChannel(string name);

		public abstract UserBase GetUser(string nickname);

		public abstract void Restart();

		public abstract void Unload();

		public abstract void Load();

		public abstract void Stop();

		public abstract void Dispose();

		public abstract SettingsBase LoadSettings();

		public DomainManager Domains
		{
			get { return _Domains; }
		}

		public ServerBase()
		{
		}

		protected ServerBase(DomainManager domains, string id)
		{
			Id = id;
			_Domains = domains;
			Channels = new ConcurrentDictionary<string, ChannelBase>();
			Users = new ConcurrentDictionary<HostMask, UserBase>();
			Created = DateTime.Now;
		}

		protected ServerBase(DomainManager domains, ServerBase clone)
		{
			Id = clone.Id;
			Settings = clone.Settings;
			_Domains = domains;
			Running = clone.Running;
			Channels = clone.Channels;
			Users = clone.Users;
			Created = clone.Created;
		}
	}
}