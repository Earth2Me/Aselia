using System;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(JoinHandler.CMD, Authorizations.Normal, ":{0} {1} {2}")]
	public sealed class JoinHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "JOIN";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			string[] channels = e.Arguments[0].Split(',');
			int key = 0;
			for (int i = 0; i < channels.Length; i++)
			{
				try
				{
					ChannelBase channel = e.Server.GetChannel(channels[i]);
					if (channel == null)
					{
						if (!e.Server.IsValidChannel(channels[i]))
						{
							e.User.SendNumeric(Numerics.ERR_BADCHANMASK, CMD, channels[i], ":That is an invalid channel name.");
							continue;
						}

						if (channels[i][0] == '#' && e.User.Level < Authorizations.Registered)
						{
							e.User.SendNumeric(Numerics.ERR_NOLOGIN, CMD, channels[i], ":You need to be registered to own a channel.  To create a temporary channel, prefix the channel name with + instead of #.");
							continue;
						}

						channel = e.Server.CreateChannel(channels[i]);
						if (channel.Prefixes.Count < 1)
						{
							channel.AddPrefix(e.User, '~');
							channel.SetModes(null, (string)e.Server.Settings["DefaultModes:" + channel.Name[0]]);
						}
					}
					else
					{
						if (channel.HasFlag("RegisteredOnly") && e.User.Level < Authorizations.Registered)
						{
							e.User.SendNumeric(Numerics.ERR_NOLOGIN, CMD, channels[i], ":Only registered users can join that channel.");
							Forward(e, channel);
							continue;
						}

						if (channel.Properties.ContainsKey("Key"))
						{
							if (e.Arguments.Length <= ++key)
							{
								e.User.SendNumeric(Numerics.ERR_KEYSET, CMD, channels[i], ":Key not specified for channel.");
								Forward(e, channel);
								continue;
							}
							else if (e.Arguments[key] != (string)channel.Properties["Key"])
							{
								e.User.SendNumeric(Numerics.ERR_BADCHANNELKEY, CMD, channels[i], ":Incorrect channel key specified.");
								Forward(e, channel);
								continue;
							}
						}

						if (channel.HasFlag("InviteOnly"))
						{
							if (!e.User.HasSessionFlag("Invite:" + channel.Name)
								&& !e.User.IsVoice(channel))
							{
								bool inviteExcept = false;
								for (int m = 0; m < channel.InviteExcepts.Count; m++)
								{
									if (e.User.Mask.Matches(channel.InviteExcepts[m]))
									{
										inviteExcept = true;
										break;
									}
								}
								if (!inviteExcept)
								{
									e.User.SendNumeric(Numerics.ERR_INVITEONLYCHAN, CMD, channels[i], ":That channel is marked invite-only.");
									Forward(e, channel);
									continue;
								}
							}
						}

						if (!e.User.IsVoice(channel))
						{
							bool except = false;
							for (int m = 0; m < channel.Exceptions.Count; m++)
							{
								if (e.User.Mask.Matches(channel.Exceptions[m]))
								{
									except = true;
									break;
								}
							}
							if (!except)
							{
								bool banned = false;
								for (int m = 0; m < channel.Bans.Count; m++)
								{
									if (e.User.Mask.Matches(channel.Bans[m]))
									{
										banned = true;
										break;
									}
								}
								if (banned)
								{
									e.User.SendNumeric(Numerics.ERR_BANNEDFROMCHAN, CMD, channels[i], ":You are banned from that channel.");
									Forward(e, channel);
									continue;
								}
							}
						}
					}

					e.User.AddToChannel(channel);
					channel.Broadcast(CMD, e.User);
				}
				catch (Exception ex)
				{
					e.User.SendNumeric(Numerics.ERR_UNAVAILRESOURCE, CMD, channels[i], ":Error joining channel:", ex.Message);
				}
			}
		}

		private void Forward(ReceivedCommandEventArgs e, ChannelBase from)
		{
			if (from.Properties.ContainsKey("Forward"))
			{
				ChannelBase channel = e.Server.GetChannel((string)from.Properties["Forward"]);
				if (channel == null)
				{
					return;
				}
				else
				{
					e.User.SendNumeric(Numerics.RPL_SUMMONING, CMD, from.Name, ":Forwarding to", channel.Name, "due to restricted access.");

					if (channel.HasFlag("RegisteredOnly") && e.User.Level < Authorizations.Registered)
					{
						e.User.SendNumeric(Numerics.ERR_NOLOGIN, CMD, channel.Name, ":Only registered users can join that channel.");
						return;
					}

					if (channel.Properties.ContainsKey("Key"))
					{
						e.User.SendNumeric(Numerics.ERR_KEYSET, CMD, channel.Name, ":Key not specified for channel.");
						return;
					}

					if (channel.HasFlag("InviteOnly"))
					{
						if (!e.User.HasSessionFlag("Invite:" + channel.Name)
							&& !e.User.IsVoice(channel))
						{
							bool inviteExcept = false;
							for (int m = 0; m < channel.InviteExcepts.Count; m++)
							{
								if (e.User.Mask.Matches(channel.InviteExcepts[m]))
								{
									inviteExcept = true;
									break;
								}
							}
							if (!inviteExcept)
							{
								e.User.SendNumeric(Numerics.ERR_INVITEONLYCHAN, CMD, channel.Name, ":That channel is marked invite-only.");
								return;
							}
						}
					}

					if (!e.User.IsVoice(channel))
					{
						bool except = false;
						for (int m = 0; m < channel.Exceptions.Count; m++)
						{
							if (e.User.Mask.Matches(channel.Exceptions[m]))
							{
								except = true;
								break;
							}
						}
						if (!except)
						{
							bool banned = false;
							for (int m = 0; m < channel.Bans.Count; m++)
							{
								if (e.User.Mask.Matches(channel.Bans[m]))
								{
									banned = true;
									break;
								}
							}
							if (banned)
							{
								e.User.SendNumeric(Numerics.ERR_BANNEDFROMCHAN, CMD, channel.Name, ":You are banned from that channel.");
								return;
							}
						}
					}
				}

				e.User.AddToChannel(channel);
				channel.Broadcast(CMD, e.User);
			}
		}
	}
}