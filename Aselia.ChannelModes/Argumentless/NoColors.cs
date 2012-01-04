using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes
{
	[ChannelMode(Modes.c, ModeSyntax.Never, Authorizations.Identified, '&')]
	public class NoColors : IChannelMode
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