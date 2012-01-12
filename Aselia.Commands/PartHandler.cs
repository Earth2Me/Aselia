using System;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(PartHandler.CMD, Authorizations.Unregistered, ":{1} {0} {2} :{3}")]
	public sealed class PartHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "PART";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			ChannelBase channel = e.User.GetChannel(e.Arguments[0]);
			if (channel == null)
			{
				e.User.SendNumeric(Numerics.ERR_NOTONCHANNEL, e.Arguments[0], ":You cannot leave a channel before joining it.");
				return;
			}

			channel.Broadcast(CMD, e.User, channel.Name, "Leaving.");
			channel.RemoveUser(e.User);
		}
	}
}