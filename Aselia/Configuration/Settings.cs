using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Aselia.Configuration
{
	[Serializable]
	public class Settings
	{
		private readonly static XmlSerializer Serializer = new XmlSerializer(typeof(Settings));
		[NonSerialized]
		private FileInfo _File;

		public byte NickLength { get; set; }

		public byte Backlog { get; set; }

		public List<Binding> Bindings { get; set; }

		public FileInfo File
		{
			set { _File = value; }
		}

		public Settings()
		{
		}

		public void SetDefaults()
		{
			Backlog = 10;
			NickLength = 20;

			Bindings = new List<Binding>(new Binding[]
			{
				new Binding()
				{
					Port = 6667,
					Protocol = Protocols.Traditional,
					Interface = 0L,
				},
				new Binding()
				{
					Port = 41232,
					Protocol = Protocols.Aselia,
					Interface = 0L,
				},
				new Binding()
				{
					Port = 7667,
					Protocol = Protocols.InterServer,
					Interface = 0L,
				},
			});
		}

		public static Settings Load(FileInfo file)
		{
			try
			{
				using (FileStream fs = file.OpenRead())
				{
					return (Settings)Serializer.Deserialize(fs);
				}
			}
			catch
			{
				Console.WriteLine("Invalid configuration file.");
				Environment.Exit(1);
				return null;
			}
		}

		public void Save()
		{
			FileInfo file = new FileInfo(_File.FullName + ".tmp");
			try
			{
				using (FileStream fs = file.Exists ? file.OpenWrite() : file.Create())
				{
					Serializer.Serialize(fs, this);
					fs.Flush();
				}

				file.Replace(_File.FullName, file.FullName + ".bak");
			}
			catch
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
			}
		}
	}
}