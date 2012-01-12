using System;
using System.IO;

namespace Aselia.Common.Core.Configuration
{
	[Serializable]
	public abstract class SettingsBase : MarshalByRefObject
	{
		[NonSerialized]
		private EventHandler _Modified;

		public event EventHandler Modified
		{
			add { _Modified += value; }
			remove { _Modified -= value; }
		}

		public string NetworkName { get; set; }

		public string[] Motd { get; set; }

		public int MaximumListSize { get; set; }

		public int MaximumLongListSize { get; set; }

		public int MaximumRanksSize { get; set; }

		public int MaximumLongRanksSize { get; set; }

		public byte MaximumUsernameLength { get; set; }

		public byte MaximumNicknameLength { get; set; }

		public byte MaximumChannelLength { get; set; }

		public ushort MaximumTopicLength { get; set; }

		public byte MaximumChannels { get; set; }

		public int PongTimeout { get; set; }

		public int PingTimeout { get; set; }

		public string DefaultChannelModesReg { get; set; }

		public string DefaultChannelModesTemp { get; set; }

		public string DefaultChannelModesLoc { get; set; }

		public int CacheCommitInterval { get; set; }

		public string CertificatePassword { get; set; }

		public Binding[] Bindings { get; set; }

		public KLine[] KLines { get; set; }

		public QLine[] QLines { get; set; }

		public ServerInfo[] NetworkServers { get; set; }

		public abstract void Flush();

		public abstract void Load(FileInfo file);

		public abstract void Save();

		public abstract void Reload();

		public SettingsBase()
		{
		}

		public SettingsBase(SettingsBase clone)
		{
			Load(clone);
		}

		public virtual void Load(SettingsBase clone)
		{
			NetworkName = clone.NetworkName;
			Motd = clone.Motd;
			MaximumListSize = clone.MaximumListSize;
			MaximumLongListSize = clone.MaximumLongListSize;
			MaximumRanksSize = clone.MaximumRanksSize;
			MaximumLongRanksSize = clone.MaximumLongRanksSize;
			MaximumUsernameLength = clone.MaximumUsernameLength;
			MaximumNicknameLength = clone.MaximumNicknameLength;
			MaximumChannelLength = clone.MaximumChannelLength;
			MaximumTopicLength = clone.MaximumTopicLength;
			MaximumChannels = clone.MaximumChannels;
			PongTimeout = clone.PongTimeout;
			PingTimeout = clone.PingTimeout;
			DefaultChannelModesReg = clone.DefaultChannelModesReg;
			DefaultChannelModesTemp = clone.DefaultChannelModesTemp;
			DefaultChannelModesLoc = clone.DefaultChannelModesLoc;
			CacheCommitInterval = clone.CacheCommitInterval;
			CertificatePassword = clone.CertificatePassword;
			NetworkServers = clone.NetworkServers;
			KLines = clone.KLines;
			QLines = clone.QLines;
			Bindings = clone.Bindings;
		}

		protected virtual void OnModified()
		{
			if (_Modified != null)
			{
				_Modified.Invoke(this, new EventArgs());
			}
		}
	}
}