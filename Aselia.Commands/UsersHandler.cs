using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(UsersHandler.CMD, Authorizations.Unregistered)]
	public sealed class UsersHandler : ICommand
	{
		public const string CMD = "USERS";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.ReplyMotd();
		}
	}
}