using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.O, ModeSyntax.Always, Authorizations.Identified, '~')]
	public class Owner : ChannelRank
	{
		public override char Prefix
		{
			get { return '~'; }
		}
	}
}