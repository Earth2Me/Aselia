using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.u, ModeSyntax.Never, Authorizations.Registered, '&')]
	public class Arena : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}