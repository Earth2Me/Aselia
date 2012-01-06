﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Aselia.Common.Core.Configuration;

namespace Aselia.Core.Configuration
{
	[Serializable]
	public class Settings : SettingsBase
	{
		[NonSerialized]
		private readonly BinaryFormatter Serializer = new BinaryFormatter();
		[NonSerialized]
		private FileInfo File;

		public override object this[string key]
		{
			get { return Properties[key]; }
			set { Properties[key] = value; }
		}

		public override void Flush()
		{
			try
			{
				Save();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Could not save settings: {0}", ex.Message);
			}

			try
			{
				OnModified();
			}
			catch
			{
			}
		}

		public override void Load(FileInfo file)
		{
			File = file;
			if (!file.Exists)
			{
				Console.WriteLine("Generating default configuration file.");
				LoadDefaults();
				OnModified();
				Save();
				return;
			}

			try
			{
				using (FileStream fs = file.OpenRead())
				{
					Properties = ((SettingsBase)Serializer.Deserialize(fs)).Properties;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Invalid configuration file.");
				throw new Exception("Invalid configuration file.", ex);
			}

			OnModified();
		}

		public override void Save()
		{
			FileInfo file = new FileInfo(File.FullName + ".tmp");
			try
			{
				using (FileStream fs = file.Exists ? file.OpenWrite() : file.Create())
				{
					Serializer.Serialize(fs, this);
					fs.Flush();
				}

				if (File.Exists)
				{
					file.Replace(File.FullName, file.FullName + ".bak");
				}
				else
				{
					file.MoveTo(File.FullName);
				}
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
				{ "NetworkName", "IronDust-Aselia" },
				{ "MaximumListSize", 100u },
				{ "MaximumLongListSize", 1000u },
				{ "MaximumRanksSize", 20u },
				{ "MaximumLongRanksSize", (byte)100 },
				{ "MaximumUsernameLength", (byte)16 },
				{ "MaximumNicknameLength", (byte)16 },
				{ "MaximumChannelLength", (byte)25 },
				{ "MaximumTopicLength", (ushort)256 },
				{ "MaximumChannels", (byte)50 },
				{ "DefaultChannelModes:#", "+rntpCc" },
				{ "DefaultChannelModes:+", "+ntpC" },
				{ "DefaultChannelModes:&", "+ntpC" },
				{ "Bindings", new List<Binding>()
				{
					new Binding()
					{
						Address = "127.0.0.1",
						Port = 6667,
						Backlog = 20,
						Protocol = Protocols.Traditional,
						Encrypted = false,
					},
				} },
				{ "K-", new List<KLine>()
				{
					new KLine()
					{
						Automated = true,
						Reason = "Invalid IP address.",
						Ban = new Cidr(0, 32),
					},
				} },
				{ "Q-", new List<QLine>()
				{
					new QLine()
					{
						Automated = true,
						Reason = "Impersonation of services.",
						Ban = "serv$",
					},
				} },
			};
		}

		public override void Reload()
		{
			Load(File);
		}
	}
}