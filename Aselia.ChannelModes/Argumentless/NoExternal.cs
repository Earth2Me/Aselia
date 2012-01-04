using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes
{
	[ChannelMode(Modes.n, ModeSyntax.Never, Authorizations.Identified, '@')]
	public class NoExternal : IChannelMode
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