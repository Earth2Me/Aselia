using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(ShunHandler.CMD, Authorizations.Unregistered)]
	public sealed class ShunHandler : ICommand
	{
		public const string CMD = "SHUN";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// TODO: Implement command
			e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":That command is not yet implemented.");
		}
	}
}