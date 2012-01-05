using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.o, ModeSyntax.Always, Authorizations.Registered, '&')]
	public class Operator : ChannelRank
	{
		public override char Prefix
		{
			get { return '@'; }
		}
	}
}