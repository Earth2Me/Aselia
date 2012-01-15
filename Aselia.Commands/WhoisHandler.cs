using System.Text;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(WhoisHandler.CMD, Authorizations.Unregistered)]
	public sealed class WhoisHandler : ICommand
	{
		public const string CMD = "WHOIS";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			UserBase user = e.Server.GetUser(e.Arguments[0]);
			if (user == null)
			{
				e.User.SendNumeric(Numerics.ERR_NOSUCHNICK, e.Arguments[0], ":That user is not online.");
				return;
			}

			e.User.SendNumeric(Numerics.RPL_WHOISUSER, user.Mask.Nickname, user.Mask.Username, user.Mask.Hostname, "*", ":" + user.Gecos);

			// TODO: RPL_WHOISSERVER

			switch (e.User.Level)
			{
			case Authorizations.NetworkOperator:
				e.User.SendNumeric(Numerics.RPL_WHOISOPERATOR, user.Mask.Nickname, ":is a network operator.");
				break;

			case Authorizations.Service:
				e.User.SendNumeric(Numerics.RPL_WHOISOPERATOR, user.Mask.Nickname, ":is a network service.");
				break;
			}

			StringBuilder channels = new StringBuilder();
			foreach (ChannelBase c in user.Channels.Values)
			{
				if (e.User.Level < Authorizations.NetworkOperator && c.HasFlag("Secret") && !e.User.IsVoice(c) && !e.User.Channels.ContainsKey(c.Id))
				{
					continue;
				}

				if (channels.Length > 0)
				{
					channels.Append(' ');
				}

				string prefix = c.GetPrefix(user);
				if (!string.IsNullOrEmpty(prefix))
				{
					if (e.User.HasSessionFlag("MultiPrefix"))
					{
						channels.Append(prefix);
					}
					else
					{
						channels.Append(prefix[0]);
					}
				}

				channels.Append(c.Name);
			}
			if (channels.Length > 0)
			{
				e.User.SendNumeric(Numerics.RPL_WHOISCHANNELS, user.Mask.Nickname, ":" + channels.ToString());
			}

			if (user.Level >= Authorizations.Registered)
			{
				e.User.SendNumeric(Numerics.RPL_WHOISACCOUNT, user.Mask.Nickname, user.Mask.Account, ":is logged in as");
			}

			e.User.SendNumeric(Numerics.RPL_ENDOFWHOIS, user.Mask.Nickname, ":End of user information.");
		}
	}
}