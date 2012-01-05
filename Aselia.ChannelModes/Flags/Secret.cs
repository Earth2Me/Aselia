using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.s, ModeSyntax.Never, Authorizations.Registered, '@')]
	public class Secret : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}