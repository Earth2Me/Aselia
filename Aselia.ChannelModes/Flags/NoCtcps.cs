using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.C, ModeSyntax.Never, Authorizations.Registered, '@')]
	public class NoCtcps : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}