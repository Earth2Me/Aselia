using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.m, ModeSyntax.Never, Authorizations.Registered, '@')]
	public class Moderated : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}