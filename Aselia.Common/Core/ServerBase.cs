using System;
using System.Collections.Generic;
using System.Net;
using Aselia.Common.Core.Configuration;
using Aselia.Common.Hotswap;
using Aselia.Common.Security;

namespace Aselia.Common.Core
{
	[Serializable]
	public abstract class ServerBase : MarshalByRefObject, IDisposable
	{
		[NonSerialized]
		private DomainManager _Domains;

		public abstract CertificateManagerBase Certificates { get; set; }

		public abstract Version CoreVersion { get; set; }

		public abstract string CoreName { get; set; }

		public DateTime Created { get; set; }

		public string Id { get; set; }

		public bool IsRunning { get; set; }

		public CacheSurrogate Cache { get; set; }

		public IDictionary<string, ChannelBase> Channels { get; set; }

		public IDictionary<HostMask, UserBase> UsersByMask { get; set; }

		public IDictionary<string, List<UserBase>> UsersByAccount { get; set; }

		public IDictionary<string, UserBase> UsersById { get; set; }

		public List<UserBase> NetworkOperators { get; set; }

		public List<UserBase> LocalUsers { get; set; }

		public SettingsBase Settings { get; set; }

		public string NetworkName { get; set; }

		public DomainManager Domains
		{
			get { return _Domains; }
		}

		public abstract bool CommitCache();

		public abstract bool Register(UserBase user, byte[] password, string email);

		public abstract bool LogIn(UserBase user, string account, byte[] password);

		public abstract void Commit(UserBase user);

		public abstract void Commit(ChannelBase channel);

		public abstract ChannelBase GetChannel(string name);

		public abstract bool IsValidChannel(string name);

		public abstract bool IsQLined(string nickname);

		public abstract bool IsKLined(IPAddress fullIp);

		public abstract ChannelBase CreateChannel(string name, UserBase user);

		public abstract UserBase GetUser(string nickname);

		public abstract void Restart();

		public abstract void Unload();

		public abstract void Load();

		public abstract void Stop();

		public abstract void Dispose();

		public abstract void LocalRehash();

		public abstract void GlobalRehash();

		public abstract SettingsBase InitializeSettings();

		public abstract UserSurrogate GetRegisteredUser(string account);

		public abstract ChannelSurrogate GetRegisteredChannel(string name);

		public ServerBase()
		{
		}

		protected ServerBase(DomainManager domains, string id)
		{
			Id = id;
			_Domains = domains;
			Channels = new Dictionary<string, ChannelBase>();
			UsersByMask = new Dictionary<HostMask, UserBase>();
			UsersByAccount = new Dictionary<string, List<UserBase>>();
			UsersById = new Dictionary<string, UserBase>();
			Created = DateTime.Now;
			IsRunning = true;
			LocalUsers = new List<UserBase>();
			NetworkOperators = new List<UserBase>();
		}

		protected ServerBase(DomainManager domains, ServerBase clone)
		{
			Id = clone.Id;
			Settings = clone.Settings;
			_Domains = domains;
			IsRunning = clone.IsRunning;
			Channels = clone.Channels;
			UsersByMask = clone.UsersByMask;
			UsersById = clone.UsersById;
			UsersByAccount = clone.UsersByAccount;
			Created = clone.Created;
			NetworkName = clone.NetworkName;
			Certificates = clone.Certificates;
			Cache = clone.Cache;
			LocalUsers = clone.LocalUsers;
			NetworkOperators = clone.NetworkOperators;
		}
	}
}