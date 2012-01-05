using System;
using Aselia.Common.Core;

namespace Aselia.Common.Modules
{
	public delegate void ReceivedCommandEventHandler(object sender, ReceivedCommandEventArgs e);

	public class ReceivedCommandEventArgs : EventArgs
	{
		public ServerBase Server { get; private set; }

		public UserBase User { get; private set; }

		public string Command { get; private set; }

		public string[] Arguments { get; private set; }

		public ReceivedCommandEventArgs(ServerBase server, UserBase user, string command, string[] arguments)
		{
			Server = server;
			User = user;
			Command = command;
			Arguments = arguments;
		}
	}
}