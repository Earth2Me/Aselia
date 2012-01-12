using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(MotdHandler.CMD, Authorizations.Unregistered)]
	public sealed class MotdHandler : ICommand
	{
		public const string CMD = "MOTD";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.ReplyMotd();
		}
	}
}