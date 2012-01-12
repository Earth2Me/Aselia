using System;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(NoticeHandler.CMD, Authorizations.Unregistered, ":{1} {0} {2} :{3}")]
	public sealed class NoticeHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "NOTICE";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 2)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			ChannelBase channel = e.Server.GetChannel(e.Arguments[0]);
			if (channel == null)
			{
				char c = e.Arguments[0][0];
				if (c == '#' || c == '&' || c == '+')
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHCHANNEL, e.Arguments[0], ":That channel does not exist.");
					return;
				}

				UserBase user = e.Server.GetUser(e.Arguments[0]);
				if (user == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHNICK, e.Arguments[0], ":That user does not exist.");
					return;
				}

				user.SendCommand(CMD, e.User.Mask, user.Mask.Nickname, e.Arguments[1]);
			}
			else
			{
				if (!e.User.IsVoice(channel) && !e.User.CanSendToChannel(channel, false, "talk"))
				{
					return;
				}

				if (channel.HasFlag("NoColors") && e.Arguments[1].Contains(((char)3).ToString()))
				{
					e.User.SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, CMD, channel.Name, ":Colors are disallowed on this channel.  Your message was blocked.");
					return;
				}

				if (channel.HasFlag("NoCtcps") && e.Arguments[1][0] == 1)
				{
					e.User.SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, CMD, channel.Name, ":CTCP replies are disallowed on this channel.  Your message was blocked.");
					return;
				}

				channel.Broadcast(CMD, e.User, channel.Name, e.Arguments[1]);
			}
		}
	}
}