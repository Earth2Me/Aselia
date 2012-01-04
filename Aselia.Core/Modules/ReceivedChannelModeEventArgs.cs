using Aselia.Optimized;

namespace Aselia.Modules
{
	public delegate void ReceivedChannelModeEventHandler(object sender, ReceivedChannelModeEventArgs e);

	public class ReceivedChannelModeEventArgs : CancelableEventArgs
	{
		public Server Server { get; private set; }

		public Channel Channel { get; private set; }

		public User User { get; private set; }

		public string Argument { get; private set; }

		public ReceivedChannelModeEventArgs(Server server, Channel channel, User user, string argument)
		{
			Server = server;
			Channel = channel;
			User = user;
			Argument = argument;
		}
	}
}