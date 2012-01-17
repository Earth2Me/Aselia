using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.UserModes.Flags
{
	[UserMode(Modes.i, ModeSyntax.Never, Authorizations.Unregistered)]
	public sealed class Invisible : UserFlag
	{
		public override string Flag
		{
			get { return "Invisible"; }
		}
	}
}