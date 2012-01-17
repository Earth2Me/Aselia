using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.UserModes.Flags
{
	[UserMode(Modes.Z, ModeSyntax.Never, Authorizations.Service)]
	public sealed class Secure : UserFlag
	{
		public override string Flag
		{
			get { return "Secure"; }
		}
	}
}