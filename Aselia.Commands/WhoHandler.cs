using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(WhoHandler.CMD, Authorizations.Unregistered)]
	public sealed class WhoHandler : ICommand
	{
		public const string CMD = "WHO";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// TODO: Implement command
			e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":That command is not yet implemented.");
		}
	}
}