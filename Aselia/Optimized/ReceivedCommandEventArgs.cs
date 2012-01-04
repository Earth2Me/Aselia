using System;

namespace Aselia.Optimized
{
	public delegate void ReceivedCommandEventHandler(object sender, ReceivedCommandEventArgs e);

	public class ReceivedCommandEventArgs : EventArgs
	{
		public Server Server { get; private set; }

		public User User { get; private set; }

		public Commands Command { get; private set; }

		public string[] Arguments { get; private set; }

		public ReceivedCommandEventArgs(Server server, User user, Commands command, string[] arguments)
		{
			Server = server;
			User = user;
			Command = command;
			Arguments = arguments;
		}
	}
}