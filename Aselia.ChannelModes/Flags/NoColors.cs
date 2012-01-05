using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.c, ModeSyntax.Never, Authorizations.Registered, '@')]
	public class NoColors : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}