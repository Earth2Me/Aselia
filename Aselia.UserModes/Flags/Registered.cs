using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.UserModes.Flags
{
	[UserMode(Modes.r, ModeSyntax.Never, Authorizations.Service)]
	public sealed class Registered : UserFlag
	{
		public override string Flag
		{
			get { return "Registered"; }
		}
	}
}