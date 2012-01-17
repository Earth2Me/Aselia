using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.UserModes.Flags
{
	[UserMode(Modes.S, ModeSyntax.Never, Authorizations.Unregistered)]
	public sealed class Service : UserFlag
	{
		public override string Flag
		{
			get { return "Service"; }
		}
	}
}