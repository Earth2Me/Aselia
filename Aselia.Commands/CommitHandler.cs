using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(CommitHandler.CMD, Authorizations.NetworkOperator)]
	public sealed class CommitHandler : ICommand
	{
		public const string CMD = "COMMIT";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.SendNumeric(Numerics.RPL_COMMITSTART, ":Committing cache...");
			try
			{
				e.Server.CommitCache();
				e.User.SendNumeric(Numerics.RPL_COMMITSUCCESS, ":The cache was successfully committed.");
			}
			catch (Exception ex)
			{
				e.User.SendNumeric(Numerics.ERR_COMMITFAIL, ":The commit failed:", ex.Message);
			}
		}
	}
}