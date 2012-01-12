using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(UserHandler.CMD, Authorizations.Connecting)]
	public sealed class UserHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "USER";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 4)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			if (e.User.Level != Authorizations.Connecting || e.User.HasSessionFlag("PassedUser"))
			{
				e.User.ErrorAlreadyRegistered(CMD);
				return;
			}

			e.Server.UsersByMask.Remove(e.User.Mask);
			e.User.Mask.Username = e.User.MakeUsername(e.Arguments[0]);
			e.Server.UsersByMask.Add(e.User.Mask, e.User);

			e.User.Gecos = e.Arguments[3];

			if (!e.User.HasSessionFlag("PassedNick") || e.User.HasSessionFlag("WaitForCap"))
			{
				e.User.SetSessionFlag("PassedUser");
				return;
			}

			e.User.ClearSessionFlag("PassedNick");
			e.User.OnConnected();
		}
	}
}