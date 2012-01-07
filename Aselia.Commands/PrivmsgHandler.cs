using System;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(PrivmsgHandler.CMD, Authorizations.Normal, ":{1} {0} {2} :{3}")]
	public sealed class PrivmsgHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "PRIVMSG";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 2)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			ChannelBase channel = e.User.GetChannel(e.Arguments[0]);
			if (channel == null)
			{
				char c = e.Arguments[0][0];
				if (c == '#' || c == '&' || c == '+')
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHCHANNEL, CMD, e.Arguments[0], ":That channel does not exist.");
					return;
				}

				UserBase user = e.Server.GetUser(e.Arguments[0]);
				if (user == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHNICK, CMD, e.Arguments[0], ":That user does not exist.");
					return;
				}

				user.SendCommand(CMD, e.User.Mask, user.Mask.Nickname, e.Arguments[1]);
			}
			else
			{
				if (!e.User.IsVoice(channel))
				{
					if (channel.HasFlag("Muted"))
					{
						e.User.SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, CMD, channel.Name, ":That channel is muted.");
						return;
					}

					for (int i = 0; i < channel.Quiets.Count; i++)
					{
						if (e.User.Mask.Matches(channel.Quiets[i]))
						{
							e.User.SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, CMD, channel.Name, ":You are muted in that channel.");
							return;
						}
					}
				}

				if (channel.HasFlag("NoColors") && e.Arguments[1].Contains(((char)3).ToString()))
				{
					e.User.SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, CMD, channel.Name, ":Colors are disallowed on this channel.  Your message was blocked.");
					return;
				}

				if (channel.HasFlag("NoCtcps") && e.Arguments[1][0] == 1 && !e.Arguments[1].StartsWith("\x0001ACTION"))
				{
					e.User.SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, CMD, channel.Name, ":CTCPs are disallowed on this channel.  Your message was blocked.");
					return;
				}

				if (channel.HasFlag("NoActions") && e.Arguments[1].StartsWith("\x0001ACTION"))
				{
					e.User.SendNumeric(Numerics.ERR_CANNOTSENDTOCHAN, CMD, channel.Name, ":Actions are disallowed on this channel.  Your message was blocked.");
					return;
				}

				channel.Broadcast(CMD, e.User, channel.Name, e.Arguments[1]);
			}
		}
	}
}