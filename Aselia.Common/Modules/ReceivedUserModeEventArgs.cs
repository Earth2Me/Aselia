using Aselia.Common.Core;

namespace Aselia.Common.Modules
{
	public delegate void ReceivedUserModeEventHandler(object sender, ReceivedUserModeEventArgs e);

	public class ReceivedUserModeEventArgs : CancelableEventArgs
	{
		public ServerBase Server { get; private set; }

		public UserBase Sender { get; private set; }

		public UserBase Target { get; private set; }

		public string Argument { get; private set; }

		public ReceivedUserModeEventArgs(ServerBase server, UserBase sender, UserBase target, string argument)
		{
			Server = server;
			Sender = sender;
			Target = target;
			Argument = argument;
		}
	}
}