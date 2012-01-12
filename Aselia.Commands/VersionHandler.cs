using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(VersionHandler.CMD, Authorizations.Unregistered)]
	public sealed class VersionHandler : ICommand
	{
		public const string CMD = "VERSION";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.ReplyVersion();
		}
	}
}