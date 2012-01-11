using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(NickHandler.CMD, Authorizations.Connecting, ":{1} {0} {2}")]
	public sealed class NickHandler : MarshalByRefObject, ICommand
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
			if (e.Server.UsersById.ContainsKey(id))
			{
				e.User.SendNumeric(Numerics.ERR_NICKNAMEINUSE, nickname, ":That nickname is already in use.");
				return;
			}

			if (e.User.Level != Authorizations.Connecting)
			{
				e.User.BroadcastInclusive("NICK", e.User.Mask.Nickname, nickname);

				e.Server.UsersByMask.Remove(e.User.Mask);
				e.Server.UsersById.Remove(e.User.Id);

				e.User.Mask.Nickname = nickname;
				e.User.Id = id;

				e.Server.UsersByMask[e.User.Mask] = e.User;
				e.Server.UsersById[e.User.Id] = e.User;
			}
			else
			{
				e.User.Mask.Nickname = nickname;
				e.User.Id = id;
			}
		}
	}
}