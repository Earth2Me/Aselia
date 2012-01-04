using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Aselia.Configuration;
using Aselia.Modules;
using Aselia.Optimized;

namespace Aselia
{
	public class Server : MarshalByRefObject, IDisposable
	{
		public bool Running { get; set; }

		public string Id { get; private set; }

		public DomainManager Domains { get; private set; }

		public Settings Settings { get; private set; }

		public ConcurrentDictionary<string, Channel> Channels { get; private set; }

		public List<User> Users { get; private set; }

		public Server(Settings settings)
		{
			Id = Environment.MachineName;
			Settings = settings;
			Channels = new ConcurrentDictionary<string, Channel>();
			Users = new List<User>();
			Domains = new DomainManager(this);
		}

		public void Dispose()
		{
		}

		public void Run()
		{
			Running = true;

			Load();

			while (Running)
			{
				Thread.Sleep(0);
			}
		}

		private void Load()
		{
			for (; ; )
			{
				try
				{
					Domains.Load();
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error loading component: " + ex.Message);
					Console.WriteLine("Retrying in 30 seconds.");
					Thread.Sleep(30000);
				}
			}
		}
	}
}