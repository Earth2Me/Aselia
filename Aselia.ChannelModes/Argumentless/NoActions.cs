using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes
{
	[ChannelMode(Modes.M, ModeSyntax.Never, Authorizations.Identified, '&')]
	public class NoActions : IChannelMode
	{
		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.User.NotImplemented();
			e.Cancel();
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.User.NotImplemented();
			e.Cancel();
		}
	}
}