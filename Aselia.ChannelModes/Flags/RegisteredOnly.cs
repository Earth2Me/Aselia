using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.r, ModeSyntax.Never, Authorizations.Registered, '&')]
	public class RegisteredOnly : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}