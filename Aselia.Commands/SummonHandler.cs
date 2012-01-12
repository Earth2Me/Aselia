using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(SummonHandler.CMD, Authorizations.Unregistered)]
	public sealed class SummonHandler : ICommand
	{
		public const string CMD = "SUMMON";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.SendNumeric(Numerics.ERR_SUMMONDISABLED, ":Summoning is disabled on this network.");
		}
	}
}