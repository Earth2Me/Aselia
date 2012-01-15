
namespace Aselia.UserCommands
{
	[Command("WHO WHOWAS", Authorizations.Unregistered)]
	public sealed class DisabledHandler : ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// TODO: Implement command
			e.User.SendNumeric(Numerics.ERR_COMMANDDISABLED, e.Command, ":That command is disabled on this network.");
		}
	}
}