using Aselia.Flags;
using Aselia.Modules;

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