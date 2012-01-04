using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.o, ModeSyntax.Always, Authorizations.Identified, '&')]
	public class Operator : ChannelRank
	{
		public override char Prefix
		{
			get { return '@'; }
		}
	}
}