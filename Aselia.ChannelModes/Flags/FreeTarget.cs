using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.F, ModeSyntax.Never, Authorizations.Registered, '&')]
	public class FreeTarget : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}