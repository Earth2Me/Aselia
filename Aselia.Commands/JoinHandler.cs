using System;
using System.Text;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(JoinHandler.CMD, Authorizations.Unregistered, ":{1} {0} :{2}")]
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
						if (e.Server.GetRegisteredChannel(channels[i]) != null)
						{
							channel = e.Server.CreateChannel(channels[i], e.User);
						}
						else
						{
							e.User.SendNumeric(Numerics.ERR_NOSUCHCHANNEL, ":That channel does not exist.  Did you mean to create/register a new channel?  If so, use /cjoin " + channels[i] + " or /quote cjoin " + channels[i] + ".");
							continue;
						}
					}

					if (channel.HasFlag("RegisteredOnly") && e.User.Level < Authorizations.Registered)
					{
						e.User.SendNumeric(Numerics.ERR_NOLOGIN, ":Only registered users can join that channel.");
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
						if (e.User.IsBanned(channel))
						{
							e.User.SendNumeric(Numerics.ERR_BANNEDFROMCHAN, channels[i], ":You are banned from that channel.");
							Forward(e, channel);
							continue;
						}
					}

					if (!e.User.AddToChannel(channel))
					{
						e.User.SendNumeric(Numerics.ERR_USERONCHANNEL, e.User.Mask.Nickname, channel.Name, ":You are already on that channel.");
						return;
					}

					OnJoin(channel, e.User);
				}
				catch (Exception ex)
				{
					e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, ":Error joining channel:", ex.Message);
				}
			}
		}

		private void OnJoin(ChannelBase channel, UserBase user)
		{
			channel.BroadcastInclusive(CMD, user, channel.Name);
			user.Names(channel);

			if (channel.Properties.ContainsKey("Topic"))
			{
				user.SendNumeric(Numerics.RPL_TOPIC, channel.Name, ":" + (string)channel.Properties["Topic"]);
			}
			else
			{
				user.SendNumeric(Numerics.RPL_NOTOPIC, channel.Name);
			}

			if (channel.Users.Count < 2)
			{
				return;
			}

			string prefix = channel.GetPrefix(user);
			if (!string.IsNullOrEmpty(prefix))
			{
				StringBuilder modes = new StringBuilder(prefix.Length + 1).Append('+');
				StringBuilder args = new StringBuilder(prefix.Length * (user.Mask.Nickname.Length + 1));
				foreach (char c in prefix)
				{
					char m;
					switch (c)
					{
					case '$':
						m = 'X';
						break;

					case '~':
						m = 'O';
						break;

					case '&':
						m = 'a';
						break;

					case '@':
						m = 'o';
						break;

					case '%':
						m = 'h';
						break;

					case '+':
						m = 'v';
						break;

					case '!':
						m = 'x';
						break;

					default:
						continue;
					}

					modes.Append(m);
					args.Append(' ').Append(user.Mask.Nickname);
				}

				channel.BroadcastInclusive("MODE", null, channel.Name, modes.ToString() + args.ToString());
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

				if (!e.User.AddToChannel(channel))
				{
					e.User.SendNumeric(Numerics.ERR_UNKNOWNERROR, CMD, channel.Name, ":Unknown error occurred while joining channel.");
				}

				OnJoin(channel, e.User);
			}
		}
	}
}