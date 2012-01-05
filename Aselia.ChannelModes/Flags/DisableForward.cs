using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.Q, ModeSyntax.Never, Authorizations.Registered, '&')]
	public class DisableForward : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}