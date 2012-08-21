using System.Linq;
using System.Collections.Generic;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(MetaHandler.CMD, Authorizations.Unregistered)]
	public sealed class MetaHandler : ICommand
	{
		public const string CMD = "META";
		public const string SYNTAX = ":Syntax: /meta <channel|account> [[+|-]<key> [value]]  (Warning!  Keys and values are case-SENSITIVE!)";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.SendNumeric(Numerics.ERR_NEEDMOREPARAMS, CMD, SYNTAX);
				return;
			}

			if (Protocol.CHANNEL_PREFIX_CHARS.Contains(e.Arguments[0][0]))
			{
				ChannelSurrogate target = e.Server.GetRegisteredChannel(e.Arguments[0]);
				if (target == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHCHANNEL, e.Arguments[0], ":That channel is not registered.");
					return;
				}

				if (e.Arguments.Length < 2)
				{
					SendMeta(e.User, target.Name, e.User.IsOwner(target) || e.User.Level >= Authorizations.NetworkOperator, target.Flags, target.Properties);
				}
				else
				{
					char dir = e.Arguments[1][0];
					string key;
					if ((dir != '-' && dir != '+') || e.Arguments[1].Length < 2)
					{
						key = e.Arguments[1];
						dir = '+';
					}
					else
					{
						key = e.Arguments[1].Substring(1);
					}
					if (key[0] != ':')
					{
						key = ":" + key;
					}

					if (e.Arguments.Length < 3)
					{
						SetFlag(e.User, dir, key, target.Flags, target.Properties);
					}
					else
					{
						SetProperty(e.User, dir, key, target.Flags, target.Properties, e.Arguments[2]);
					}
					target.Commit();
				}
			}
			else
			{
				UserSurrogate target = e.Server.GetRegisteredUser(e.Arguments[0]);
				if (target == null)
				{
					e.User.SendNumeric(Numerics.ERR_NOSUCHACCOUNT, e.Arguments[0], ":That account is not registered.");
					return;
				}

				if (e.Arguments.Length < 2)
				{
					SendMeta(e.User, target.Mask.Account, e.User.Mask.Account == e.Arguments[0].ToLower() || e.User.Level >= Authorizations.NetworkOperator, target.Flags, target.Properties);
				}
				else
				{
					char dir = e.Arguments[1][0];
					string key;
					if ((dir != '-' && dir != '+') || e.Arguments[1].Length < 2)
					{
						key = e.Arguments[1];
						dir = '+';
					}
					else
					{
						key = e.Arguments[1].Substring(1);
					}
					if (key[0] != ':')
					{
						key = ":" + key;
					}

					if (e.Arguments.Length < 3)
					{
						SetFlag(e.User, dir, key, target.Flags, target.Properties);
					}
					else
					{
						SetProperty(e.User, dir, key, target.Flags, target.Properties, e.Arguments[2]);
					}
					target.Commit();
				}
			}
		}

		private void SetProperty(UserBase user, char dir, string key, List<string> flags, IDictionary<string, object> properties, string value)
		{
			switch (dir)
			{
			case '+':
				properties[key] = value;
				user.SendNumeric(Numerics.RPL_PROPERTYSET, key, ":Property set.");
				break;

			case '-':
				flags.Remove(key);
				properties.Remove(key);
				user.SendNumeric(Numerics.RPL_FLAGSET, key, ":Flag/property cleared.");
				break;
			}
		}

		private void SetFlag(UserBase user, char dir, string key, List<string> flags, IDictionary<string, object> properties)
		{
			switch (dir)
			{
			case '+':
				if (!flags.Contains(key))
				{
					flags.Add(key);
				}
				user.SendNumeric(Numerics.RPL_FLAGSET, key, ":Flag set.");
				break;

			case '-':
				flags.Remove(key);
				properties.Remove(key);
				user.SendNumeric(Numerics.RPL_FLAGSET, key, ":Flag/property cleared.");
				break;
			}
		}

		private void SendMeta(UserBase user, string target, bool super, List<string> allFlags, IDictionary<string, object> allProperties)
		{
			List<string> flags;
			if (super)
			{
				flags = allFlags;
			}
			else
			{
				flags = new List<string>();
				for (int i = 0; i < allFlags.Count; i++)
				{
					switch (allFlags[i])
					{
					case "Arena":
					case "DisableForward":
					case "FreeTarget":
					case "InviteOnly":
					case "LockTopic":
					case "LongLists":
					case "Moderated":
					case "NoActions":
					case "NoColors":
					case "NoCtcps":
					case "NoExternal":
					case "OpModerated":
					case "Permanent":
					case "Private":
					case "RegisteredOnly":
					case "Secret":
						flags.Add(flags[i]);
						continue;

					default:
						if (allFlags[i][0] == ':')
						{
							flags.Add(flags[i]);
						}
						continue;
					}
				}
			}

			user.SendNumeric(Numerics.RPL_METAFLAGS, target, string.Join(" ", flags));

			foreach (KeyValuePair<string, object> kv in allProperties)
			{
				if (!super)
				{
					switch (kv.Key)
					{
					case "JoinThrottle":
					case "Limit":
					case "Forward":
					case "Created":
					case "Founder":
						break;

					default:
						if (kv.Key[0] == ':')
						{
							break;
						}
						continue;
					}
				}

				user.SendNumeric(Numerics.RPL_METAPROPERTY, target, kv.Key, "=", kv.Value);
			}
		}
	}
}