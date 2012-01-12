using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(DemoteHandler.CMD, Authorizations.NetworkOperator)]
	public sealed class DemoteHandler : ICommand
	{
		public const string CMD = "DEMOTE";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			UserSurrogate user = e.Server.GetRegisteredUser(e.Arguments[0]);
			if (user == null)
			{
				e.User.SendNumeric(Numerics.ERR_NOSUCHACCOUNT, e.Arguments[0].ToLower(), ":That account is not registered.");
				return;
			}

			user.Level = Authorizations.NetworkOperator;
			user.Commit();
			e.User.SendNumeric(Numerics.RPL_DEMOTED, user.Mask.Account, ":That user is no longer an IRCop.");
		}
	}
}