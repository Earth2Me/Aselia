using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(RegainHandler.CMD, Authorizations.Registered)]
	public sealed class RegainHandler : ICommand
	{
		public const string CMD = "REGAIN";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// TODO: Implement command
			e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":That command is not yet implemented.");
		}
	}
}