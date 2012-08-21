using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	// TODO: Implement these commands.
	[Command("WHO WHOWAS", Authorizations.Unregistered)]
	public sealed class DisabledHandler : ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.SendNumeric(Numerics.ERR_COMMANDDISABLED, e.Command, ":That command is disabled on this server.");
		}
	}
}