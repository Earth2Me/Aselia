using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.UserModes.Flags
{
	[UserMode(Modes.o, ModeSyntax.Never, Authorizations.Service)]
	public sealed class Operator : UserFlag
	{
		public override string Flag
		{
			get { return "Operator"; }
		}
	}
}