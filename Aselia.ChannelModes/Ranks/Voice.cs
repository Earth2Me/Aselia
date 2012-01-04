using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.v, ModeSyntax.Always, Authorizations.Identified, '%')]
	public class Voice : ChannelRank
	{
		public override char Prefix
		{
			get { return '+'; }
		}
	}
}