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
#if !DEBUG
				Save();
#endif
				return;
			}

			try
			{
				using (FileStream fs = file.OpenRead())
				{
					Load(((Settings)Serializer.Deserialize(fs)));
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
			NetworkName = "Earth2Me";
			MaximumListSize = 100;
			MaximumLongListSize = 1000;
			MaximumRanksSize = 20;
			MaximumLongRanksSize = 100;
			MaximumUsernameLength = 16;
			MaximumNicknameLength = 16;
			MaximumChannelLength = 30;
			MaximumTopicLength = 300;
			MaximumChannels = 50;
			PongTimeout = 20000;
			PingTimeout = 120000;
			DefaultChannelModesReg = "+rntpCc";
			DefaultChannelModesTemp = "+ntpCc";
			DefaultChannelModesLoc = "+ntpCc";
			CacheCommitInterval = 300000;
			CertificatePassword = "3F3fB2K3JFN2N2CU23v*ZHv&#(@b@#fnn3B@nf*vh@(V@nv(vHvh&@n@v* (!(@nvw(eughw)gDV(w_e_e@f@nc!efkq!fmw)vwd_vlfl!#f{g}|!";
			NetworkServers = new ServerInfo[]
			{
				new ServerInfo()
				{
					Id = Environment.MachineName,
					InterServerPort = 51232,
					InterServerIp = "127.0.0.1",
				},
			};
			Bindings = new Binding[]
			{
				new Binding()
				{
					Interfaces = new string[]
					{
						"127.0.0.1",
					},
					Port = 6667,
					Backlog = 20,
					Protocol = Protocols.Rfc2812,
				},
				new Binding()
				{
					Interfaces = new string[]
					{
						"127.0.0.1",
					},
					Port = 7000,
					Backlog = 20,
					Protocol = Protocols.Rfc2812Ssl,
				},
				new Binding()
				{
					Interfaces = new string[]
					{
						"127.0.0.1",
					},
					Port = 51232,
					Backlog = 20,
					Protocol = Protocols.InterServer,
				},
			};
			KLines = new List<KLine>(new KLine[]
			{
				new KLine()
				{
					Automated = true,
					Reason = "Invalid IP address.",
					Ban = new Cidr(0, 32),
				},
			});
			QLines = new List<QLine>(new QLine[]
			{
				new QLine()
				{
					Automated = true,
					Reason = "Impersonation of services.",
					Ban = "serv$",
				},
			});
		}

		public override void Reload()
		{
			Load(File);
		}
	}
}