using Aselia.Common.Core;

namespace Aselia.Common.Modules
{
	public delegate void ReceivedChannelModeEventHandler(object sender, ReceivedChannelModeEventArgs e);

	public class ReceivedChannelModeEventArgs : CancelableEventArgs
	{
		public ServerBase Server { get; private set; }

		public ChannelBase Channel { get; private set; }

		public UserBase User { get; private set; }

		public string Argument { get; private set; }

		public ReceivedChannelModeEventArgs(ServerBase server, ChannelBase channel, UserBase user, string argument)
		{
			Server = server;
			Channel = channel;
			User = user;
			Argument = argument;
		}
	}
}