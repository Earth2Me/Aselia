using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.a, ModeSyntax.Always, Authorizations.Identified, '~')]
	public class Protect : ChannelRank
	{
		public override char Prefix
		{
			get { return '&'; }
		}
	}
}