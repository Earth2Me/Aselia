using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.t, ModeSyntax.Never, Authorizations.Registered, 't')]
	public class LockTopic : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}