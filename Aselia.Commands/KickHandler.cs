using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(KickHandler.CMD, Authorizations.Unregistered, ":{1} {0} {2} {3} :{4}")]
	public sealed class KickHandler : ICommand
	{
		public const string CMD = "KICK";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 2)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			ChannelBase channel;
			if (e.User.Level >= Authorizations.NetworkOperator)
			{
				channel = e.Server.GetChannel(e.Arguments[0]);
				if (channel == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHCHANNEL, e.Arguments[0], ":That channel does not exist.");
					return;
				}

				if (!e.User.IsOperator(channel))
				{
					e.User.SendNumeric(Numerics.ERR_CHANOPPRIVSNEEDED, channel.Name, ":You need to be a channel operator or IRCop to kick from that channel.");
					return;
				}
			}
			else
			{
				channel = e.User.GetChannel(e.Arguments[0]);
				if (channel == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOTONCHANNEL, e.Arguments[0], ":You are not on that channel.");
					return;
				}
			}

			UserBase target = channel.GetUser(e.Arguments[1]);
			if (target == null)
			{
				e.User.SendNumeric(Numerics.ERR_USERNOTINCHANNEL, e.Arguments[1], channel.Name, ":That user is not in the specified channel.");
				return;
			}

			channel.BroadcastInclusive(CMD, e.User, channel.Name, target.Mask.Nickname, e.Arguments.Length > 2 ? e.Arguments[2] : "User kicked from channel.");
			channel.RemoveUser(target);
		}
	}
}