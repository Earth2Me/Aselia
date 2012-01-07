using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.n, ModeSyntax.Never, Authorizations.Normal, '@')]
	public class NoExternal : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}