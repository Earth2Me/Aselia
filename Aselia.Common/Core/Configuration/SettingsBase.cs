using System;
using System.Collections.Generic;
using System.IO;

namespace Aselia.Common.Core.Configuration
{
	[Serializable]
	public abstract class SettingsBase : MarshalByRefObject
	{
		public Dictionary<string, object> Properties { get; set; }

		public abstract void Load(FileInfo file);

		public abstract void Save();

		public SettingsBase()
		{
		}

		public SettingsBase(SettingsBase clone)
		{
			Properties = clone.Properties;
		}
	}
}