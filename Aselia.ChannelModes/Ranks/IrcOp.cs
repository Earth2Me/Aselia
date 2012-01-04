using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.x, ModeSyntax.Always, Authorizations.Service)]
	public sealed class IrcOp : ChannelRank
	{
		public override char Prefix
		{
			get { return '!'; }
		}
	}
}