using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(GhostHandler.CMD, Authorizations.Registered)]
	public sealed class GhostHandler : ICommand
	{
		public const string CMD = "GHOST";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// TODO: Implement command
			e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":That command is not yet implemented.");
		}
	}
}