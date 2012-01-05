using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.X, ModeSyntax.Always, Authorizations.Service)]
	public class Service : ChannelRank
	{
		public override char Prefix
		{
			get { return '$'; }
		}
	}
}