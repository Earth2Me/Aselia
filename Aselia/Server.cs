using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Aselia.Configuration;
using Aselia.Optimized;

namespace Aselia
{
	public class Server : IDisposable
	{
		public bool Running { get; set; }

		public string Id { get; private set; }

		public Settings Settings { get; private set; }

		public ConcurrentDictionary<string, Channel> Channels { get; private set; }

		public List<User> Users { get; private set; }

		public Server(Settings settings)
		{
			Id = Environment.MachineName;
			Settings = settings;
			Channels = new ConcurrentDictionary<string, Channel>();
			Users = new List<User>();
		}

		public void Dispose()
		{
		}

		public void Run()
		{
			Running = true;

			while (Running)
			{
				Thread.Sleep(0);
			}
		}
	}
}