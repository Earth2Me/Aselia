using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(TopicHandler.CMD, Authorizations.Normal, ":{1} {0} {2} :{3}")]
	public sealed class TopicHandler : ICommand
	{
		public const string CMD = "TOPIC";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			if (!e.Server.IsValidChannel(e.Arguments[0]))
			{
				e.User.SendNumeric(Numerics.ERR_BADCHANMASK, e.Arguments[0], ":That is an invalid channel mask.");
				return;
			}

			ChannelBase channel = e.Server.GetChannel(e.Arguments[0]);
			if (channel == null || (channel.HasFlag("Secret") && !e.User.IsVoice(channel) && e.User.GetChannel(channel.Name) == null))
			{
				e.User.SendNumeric(Numerics.ERR_NOSUCHCHANNEL, e.Arguments[0], ":That channel does not exist.");
				return;
			}

			if (e.Arguments.Length > 1)
			{
				if (!e.User.IsOperator(channel))
				{
					if (channel.HasFlag("TopicLock"))
					{
						e.User.SendNumeric(Numerics.ERR_CHANOPPRIVSNEEDED, channel.Name, ":You must be an operator or higher to change the topic.");
						return;
					}
					else if (!e.User.CanSendToChannel(channel, false, "change the topic"))
					{
						return;
					}
				}

				string topic = e.Arguments[1];
				if (string.IsNullOrWhiteSpace(topic) || topic == "-")
				{
					topic = string.Empty;
					if (channel.Properties.ContainsKey("Topic"))
					{
						channel.Properties.Remove("Topic");
						channel.Commit();
					}
					else
					{
						return;
					}
				}
				else
				{
					ushort max = (ushort)e.Server.Settings["MaximumTopicLength"];
					if (topic.Length > max)
					{
						topic = topic.Remove(max);
					}

					channel.Properties["Topic"] = topic;
					channel.Commit();
				}

				channel.Broadcast(CMD, e.User, channel.Name, topic);
			}
			else
			{
				if (channel.Properties.ContainsKey("Topic"))
				{
					e.User.SendNumeric(Numerics.RPL_TOPIC, channel.Name, (string)channel.Properties["Topic"]);
				}
				else
				{
					e.User.SendNumeric(Numerics.RPL_NOTOPIC, channel.Name);
				}
			}
		}
	}
}