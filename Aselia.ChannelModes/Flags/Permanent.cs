using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.P, ModeSyntax.Never, Authorizations.NetworkOperator)]
	public class Permanent : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}