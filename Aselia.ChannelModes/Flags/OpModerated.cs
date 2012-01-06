using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.z, ModeSyntax.Never, Authorizations.Normal, '@')]
	public class OpModerated : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}