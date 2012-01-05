using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.O, ModeSyntax.Always, Authorizations.Normal, '~')]
	public class Owner : ChannelRank
	{
		public override char Prefix
		{
			get { return '~'; }
		}
	}
}