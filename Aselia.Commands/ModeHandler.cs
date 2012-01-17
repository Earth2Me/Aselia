using System;
using System.Linq;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(ModeHandler.CMD, Authorizations.Unregistered, ":{1} {0} {2} {3}")]
	public sealed class ModeHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "MODE";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}
			else if (e.Arguments.Length < 2)
			{
				ChannelBase channel = e.User.GetChannel(e.Arguments[0]);
				if (channel == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOTONCHANNEL, e.Arguments[0], ":You are not on that channel.");
					return;
				}

				e.User.SendNumeric(Numerics.RPL_CHANNELMODEIS, channel.Name, channel.GetModeString());
				return;
			}

			string args;
			if (e.Arguments.Length > 2)
			{
				args = string.Join(" ", e.Arguments, 2, e.Arguments.Length - 2);
			}
			else
			{
				args = string.Empty;
			}

			foreach (string t in e.Arguments[0].Split(','))
			{
				try
				{
					if (".!#&".Contains(t[0]))
					{
						ChannelBase channel = e.User.GetChannel(t);
						if (channel == null)
						{
							e.User.SendNumeric(Numerics.ERR_NOTONCHANNEL, t, ":You are not on that channel.");
							return;
						}

						channel.SetModes(e.User, e.Arguments[1], args);
					}
					else if (t.ToLower() != e.User.Id)
					{
						e.User.SendNumeric(Numerics.ERR_COMMANDSPECIFIC, ":You can only set user mode flags on yourself.");
					}
					else
					{
						e.User.SetModes(e.User, e.Arguments[1], args);
					}
				}
				catch (Exception ex)
				{
					e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":Error setting mode:", ex.Message);
				}
			}
		}
	}
}