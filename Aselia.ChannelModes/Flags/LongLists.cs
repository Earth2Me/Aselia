using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.L, ModeSyntax.Never, Authorizations.NetworkOperator)]
	public class LongLists : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}