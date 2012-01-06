using System;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(UserhostHandler.CMD, Authorizations.Normal)]
	public sealed class UserhostHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "USERHOST";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
			}

			UserBase user = e.Server.GetUser(e.Arguments[0]);
			if (user == null)
			{
				e.User.SendNumeric(Numerics.RPL_USERHOST, ":no such user");
			}
			else
			{
				e.User.SendNumeric(Numerics.RPL_USERHOST, ":" + user.Mask.Nickname + "=+" + user.Mask.Username + "@" + user.Mask.Hostname);
			}
		}
	}
}