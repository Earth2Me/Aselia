using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(WhoisHandler.CMD, Authorizations.Unregistered)]
	public sealed class WhoisHandler : ICommand
	{
		public const string CMD = "WHOIS";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// TODO: Implement command
			e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":That command is not yet implemented.");
		}
	}
}