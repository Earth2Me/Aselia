using System;
using System.Linq;
using System.Text.RegularExpressions;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(NickHandler.CMD, Authorizations.Connecting)]
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

			char[] chars = nickname.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if (!HostMask.NICKNAME_CHARS.Contains(chars[i]))
				{
					e.User.SendNumeric(Numerics.ERR_ERRONEUSNICKNAME, nickname, "That nickname contains invalid character(s).");
					return;
				}
			}

			foreach (Regex r in e.Server.IsQLined)
			{
				if (r.IsMatch(nickname))
				{
					e.User.Send(Commands.ERR_NICKCOLLISION, e.Server.Id, e.User.Mask.Nickname, "That nickname is invalid.");
					return;
				}
			}

			string id = nickname.ToLower();
			for (int i = 0; i < e.Server.Users.Count; i++)
			{
				if (e.Server.Users[i].Id == id)
				{
					e.User.Send(Commands.ERR_NICKNAMEINUSE, e.Server.Id, e.User.Mask.Nickname, "That nickname is already in use.");
					return;
				}
			}

			if (e.User.Authorization != Authorizations.Connecting)
			{
				e.User.BroadcastInclusive(Commands.NICK, e.User, nickname);
			}

			e.User.Id = id;
			e.User.Mask.Nickname = nickname;
		}
	}
}