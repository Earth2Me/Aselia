using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(PromoteHandler.CMD, Authorizations.NetworkOperator)]
	public sealed class PromoteHandler : ICommand
	{
		public const string CMD = "PROMOTE";

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
			e.User.SendNumeric(Numerics.RPL_PROMOTED, user.Mask.Account, ":That user is now an IRCop.");
		}
	}
}