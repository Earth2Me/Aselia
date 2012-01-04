using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes
{
	[ChannelMode(Modes.j, ModeSyntax.OnAdd, Authorizations.Identified, '@')]
	public class JoinThrottle : IChannelMode
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