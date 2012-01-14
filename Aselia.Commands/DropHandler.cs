using System.Linq;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(DropHandler.CMD, Authorizations.Registered)]
	public sealed class DropHandler : ICommand
	{
		public const string CMD = "DROP";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			string id = e.Arguments[0].ToLower();
			if ("#&".Contains(id[0]))
			{
				e.User.SendNumeric(Numerics.ERR_COMMANDSPECIFIC, CMD, ":You cannot drop an unregistered channel.");
				return;
			}
			else if (".!".Contains(id[0]))
			{
				ChannelSurrogate channel = e.Server.GetRegisteredChannel(id);
				if (channel == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHCHANNEL, id, ":That channel is not registered.");
					return;
				}

				if (e.User.Level < Authorizations.NetworkOperator)
				{
					e.User.SendNumeric(Numerics.ERR_CHANOPPRIVSNEEDED, channel.Name, ":You must be a channel owner or IRCop to drop a channel.");
					return;
				}

				if (channel is ChannelBase)
				{
					ChannelBase cb = (ChannelBase)channel;
					foreach (UserBase u in cb.Users.Values)
					{
						u.SendCommand("KICK", e.User.Mask, cb.Name, u.Mask.Nickname, "Channel dropped.");
					}
					cb.Dispose();
				}

				e.Server.Cache.Channels.Remove(id);

				e.User.SendNumeric(Numerics.RPL_CHANNELDROPPED, id, ":That channel was dropped.");
			}
			else if (id != e.User.Mask.Account && e.User.Level < Authorizations.NetworkOperator)
			{
				e.User.SendNumeric(Numerics.ERR_NOPRIVILEGES, ":You need to be logged in as that user or be an IRCop to drop that account.");
				return;
			}
			else
			{
				UserSurrogate user = e.Server.GetRegisteredUser(id);
				if (user == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHACCOUNT, id, ":That account does not exist.");
					return;
				}

				user.Level = Authorizations.Unregistered;
				user.Commit();

				if (user is UserBase)
				{
					e.User.SendNumeric(Numerics.RPL_ACCOUNTDROPPED, id, ":Your account was dropped.");
				}

				if (user != e.User)
				{
					e.User.SendNumeric(Numerics.RPL_ACCOUNTDROPPED, id, ":That account was dropped.");
				}
			}
		}
	}
}