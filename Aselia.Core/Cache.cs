using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using Aselia.Common.Core;

namespace Aselia.Core
{
	[Serializable]
	public class Cache : CacheSurrogate
	{
		private static readonly FileInfo File = new FileInfo("Cache.db");
		private static readonly BinaryFormatter Serializer = new BinaryFormatter();

		public Cache()
		{
		}

		public Cache(CacheSurrogate clone)
			: base(clone)
		{
		}

		public bool Save()
		{
			FileInfo tmp = new FileInfo(File.FullName + ".tmp");
			try
			{
				using (FileStream fs = tmp.Exists ? tmp.OpenWrite() : tmp.Create())
				using (DeflateStream deflate = new DeflateStream(fs, CompressionMode.Compress))
				{
					Serializer.Serialize(fs, new CacheSurrogate(this));
				}

				FileInfo bak = new FileInfo(File.FullName + ".bak");
				bak.Delete();
				if (File.Exists)
				{
					tmp.Replace(File.FullName, bak.FullName);
				}
				else
				{
					tmp.MoveTo(File.FullName);
				}
				return true;
			}
			catch
			{
				try
				{
					tmp.Delete();
				}
				catch
				{
				}
				return false;
			}
		}

		public static Cache Create()
		{
			return new Cache()
			{
				Accounts = new ConcurrentDictionary<string, UserSurrogate>(),
				Channels = new ConcurrentDictionary<string, ChannelSurrogate>(),
			};
		}

		public static Cache Load()
		{
			try
			{
				if (File.Exists)
				{
					using (FileStream fs = File.OpenRead())
					using (DeflateStream deflate = new DeflateStream(fs, CompressionMode.Decompress))
					{
						return new Cache((CacheSurrogate)Serializer.Deserialize(fs));
					}
				}
				else
				{
					return null;
				}
			}
			catch
			{
				return null;
			}
		}
	}
}