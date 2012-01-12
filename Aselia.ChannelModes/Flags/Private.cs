using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.p, ModeSyntax.Never, Authorizations.Unregistered, '@')]
	public class Private : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}