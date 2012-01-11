using System;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(CjoinHandler.CMD, Authorizations.Normal)]
	public sealed class CjoinHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "CJOIN";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			string[] channels = e.Arguments[0].Split(',');
			for (int i = 0; i < channels.Length; i++)
			{
				try
				{
					ChannelBase channel = e.Server.GetChannel(channels[i]);
					if (channel == null)
					{
						if (!e.Server.IsValidChannel(channels[i]))
						{
							e.User.SendNumeric(Numerics.ERR_BADCHANMASK, channels[i], ":That is an invalid channel name.");
							continue;
						}

						channel = e.Server.CreateChannel(channels[i], e.User);
						if (channel == null)
						{
							continue;
						}

						if (!e.User.AddToChannel(channel))
						{
							e.User.SendNumeric(Numerics.ERR_CONCURRENCY, ":A concurrency error occurred while joining the channel.");
							return;
						}

						channel.Broadcast("JOIN", e.User, channel.Name);
						e.User.Names(channel);
						e.Server.Channels[channel.Name] = channel;
					}
					else
					{
						e.User.SendNumeric(Numerics.ERR_CHANNELEXISTS, channels[i], ":That channel already exists.");
					}
				}
				catch (Exception ex)
				{
					e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":Error joining channel:", ex.Message);
				}
			}
		}
	}
}