using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Aselia.Common.Core.Configuration;

namespace Aselia.Core.Configuration
{
	[Serializable]
	public class Settings : SettingsBase
	{
		[NonSerialized]
		private readonly XmlSerializer Serializer = new XmlSerializer(typeof(Settings));
		[NonSerialized]
		private FileInfo File;

		public override void Load(FileInfo file)
		{
			File = file;
			if (!file.Exists)
			{
				Console.WriteLine("Generating default configuration file.");
				LoadDefaults();
			}

			try
			{
				using (FileStream fs = file.OpenRead())
				{
					Properties = ((Settings)Serializer.Deserialize(fs)).Properties;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Invalid configuration file.");
				throw new Exception("Invalid configuration file.", ex);
			}
		}

		public void Save()
		{
			FileInfo file = new FileInfo(File.FullName + ".tmp");
			try
			{
				using (FileStream fs = file.Exists ? file.OpenWrite() : file.Create())
				{
					Serializer.Serialize(fs, this);
					fs.Flush();
				}

				file.Replace(File.FullName, file.FullName + ".bak");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to save configuration file.");
				if (file.Exists)
				{
					try
					{
						file.Delete();
					}
					catch
					{
					}
				}

				throw new Exception("Unable to save configuration file.", ex);
			}
		}

		private void LoadDefaults()
		{
			Properties = new Dictionary<string, object>()
			{
				{ "MaximumListSize", 100 },
				{ "MaximumLongListSize", 1000 },
				{ "MaximumRanksSize", 20 },
				{ "MaximumLongRanksSize", 100 },
				{ "Bindings", new List<Binding>()
				{
					new Binding()
					{
						Address = "127.0.0.1",
						Port = 6667,
						Backlog = 20,
						Protocol = Protocols.Traditional,
						Encrypted = false,
					}
				} },
			};
		}
	}
}