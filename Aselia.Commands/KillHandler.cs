using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(KillHandler.CMD, Authorizations.NetworkOperator, "{1} {0} {2} :{3}")]
	public sealed class KillHandler : ICommand
	{
		public const string CMD = "KILL";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			UserBase user = e.Server.GetUser(e.Arguments[0]);
			if (user == null)
			{
				e.User.SendNumeric(Numerics.ERR_NOSUCHNICK, e.Arguments[0], ":That nickname does not exist.");
				return;
			}

			string reason = e.Arguments.Length > 1 ? "Killed: " + e.Arguments[1] : "Killed by network operator.";

			e.User.SendCommand(CMD, e.Server.Id, e.User.Mask.Nickname, reason);
			e.User.Dispose(reason);
		}
	}
}