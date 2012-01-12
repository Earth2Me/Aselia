using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(LusersHandler.CMD, Authorizations.Unregistered)]
	public sealed class LusersHandler : ICommand
	{
		public const string CMD = "LUSERS";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.ReplyMotd();
		}
	}
}