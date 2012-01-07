using System;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(NickHandler.CMD, Authorizations.Connecting, ":{1} {0} {2}")]
	public class NickHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "NICK";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}
			string nickname = e.Arguments[0];

			if (e.User.Mask.Nickname != "*")
			{
				e.User.ErrorAlreadyRegistered(CMD);
				return;
			}

			if (!e.User.ValidateNickname(nickname))
			{
				// ValidateNickname will send the numeric.
				return;
			}

			if (e.Server.IsQLined(nickname))
			{
				e.User.SendNumeric(Numerics.ERR_NICKCOLLISION, nickname, ":That nickname is invalid.");
				return;
			}

			string id = nickname.ToLower();
			foreach (UserBase u in e.Server.Users.Values)
			{
				if (u.Id == id)
				{
					e.User.SendNumeric(Numerics.ERR_NICKNAMEINUSE, nickname, ":That nickname is already in use.");
					return;
				}
			}

			if (e.User.Level != Authorizations.Connecting)
			{
				e.User.BroadcastInclusive("NICK", e.User.Mask.Nickname, nickname);

				UserBase dump;
				e.Server.Users.TryRemove(e.User.Mask, out dump);
				e.User.Mask.Nickname = nickname;
				e.Server.Users[e.User.Mask] = e.User;
			}
			else
			{
				e.User.Mask.Nickname = nickname;
			}
			e.User.Id = id;
		}
	}
}