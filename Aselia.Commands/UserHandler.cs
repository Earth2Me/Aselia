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

			if (e.User.Mask.Username[0] != ':')
			{
				e.User.ErrorAlreadyRegistered(CMD);
				return;
			}

			e.User.Mask.Username = e.User.MakeUsername(e.Arguments[0]);
			e.User.Gecos = e.Arguments[3];

			if (e.User.HasSessionFlag("WaitForCap"))
			{
				return;
			}

			e.User.OnConnected();
		}
	}
}