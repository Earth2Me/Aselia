using System;
using System.Collections.Generic;
using System.IO;

namespace Aselia.Common.Core.Configuration
{
	[Serializable]
	public abstract class SettingsBase : MarshalByRefObject
	{
		public event EventHandler Modified;

		public Dictionary<string, object> Properties { get; set; }

		public abstract void Flush();

		public abstract void Load(FileInfo file);

		public abstract void Save();

		public abstract void Reload();

		public abstract object this[string key] { get; set; }

		public SettingsBase()
		{
		}

		public SettingsBase(SettingsBase clone)
		{
			Properties = clone.Properties;
		}

		protected virtual void OnModified()
		{
			if (Modified != null)
			{
				Modified.Invoke(this, new EventArgs());
			}
		}
	}
}