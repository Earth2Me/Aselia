using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

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