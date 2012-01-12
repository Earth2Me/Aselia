using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(RehashHandler.CMD, Authorizations.NetworkOperator)]
	public sealed class RehashHandler : ICommand
	{
		public const string CMD = "REHASH";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.SendNumeric(Numerics.RPL_REHASHING, ":Instructing all servers to reload their configuration files.");
			e.Server.GlobalRehash();
		}
	}
}