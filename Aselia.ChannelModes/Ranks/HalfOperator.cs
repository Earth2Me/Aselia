using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.h, ModeSyntax.Always, Authorizations.Identified, '@')]
	public class HalfOperator : ChannelRank
	{
		public override char Prefix
		{
			get { return '%'; }
		}
	}
}