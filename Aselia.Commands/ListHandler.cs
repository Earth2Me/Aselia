using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(ListHandler.CMD, Authorizations.Unregistered)]
	public sealed class ListHandler : ICommand
	{
		public const string CMD = "LIST";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// TODO: Implement command
			e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":That command is not yet implemented.");
		}
	}
}