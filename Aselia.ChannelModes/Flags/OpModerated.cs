using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modeszs, ModeSyntax.Never, Authorizations.Registered, '@')]
	public class OpModerated : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}