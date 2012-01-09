using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Aselia.Test
{
	internal class Program
	{
		private static volatile int Count;

		private static void Main(string[] args)
		{
			Thread.Sleep(10000);

			TcpClient[] clients = new TcpClient[1000000];

			for (int i = 0; i < clients.Length; i++)
			{
				clients[i] = new TcpClient();
				clients[i].BeginConnect(IPAddress.Loopback, 6667, OnBeginConnect, clients[i]);
				Thread.Sleep(100);
			}

			Console.ReadKey(false);
		}

		private static void OnBeginConnect(IAsyncResult ar)
		{
			try
			{
				TcpClient client = (TcpClient)ar.AsyncState;
				if (!client.Connected)
				{
					return;
				}
				int count = ++Count;

				StreamWriter tx = new StreamWriter(client.GetStream());
				tx.WriteLine("NICK T{0}", count);
				tx.WriteLine("USER T{0} * * :Testing");
				tx.WriteLine("JOIN #T29837");
				tx.Flush();
				Console.WriteLine(count);

				Timer timer = null;
				timer = new Timer(delegate(object state)
				{
					try
					{
						tx.WriteLine("PRIVMSG #T29837 :Hello.");
						tx.Flush();
					}
					catch
					{
						timer.Change(Timeout.Infinite, Timeout.Infinite);
						Count--;
						timer.Dispose();
					}
				});
				timer.Change(0, 190000);
			}
			catch
			{
			}
		}
	}
}