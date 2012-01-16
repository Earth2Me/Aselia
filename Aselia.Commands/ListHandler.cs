using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(ListHandler.CMD, Authorizations.Unregistered)]
	public sealed class ListHandler : ICommand
	{
		public const string CMD = "LIST";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.SendNumeric(Numerics.RPL_LISTSTART, "Channels", ":" + e.User.Mask.Nickname);
			foreach (ChannelBase c in e.Server.Channels.Values)
			{
				if (c.HasFlag("Secret") && e.User.Level < Authorizations.NetworkOperator && !e.User.IsVoice(c) && !e.User.Channels.ContainsKey(c.Id))
				{
					e.User.SendNumeric(Numerics.RPL_LIST, "*", c.Users.Count);
				}
				else if (c.Properties.ContainsKey("Topic"))
				{
					e.User.SendNumeric(Numerics.RPL_LIST, c.Name, c.Users.Count, ":" + c.Properties["Topic"]);
				}
				else
				{
					e.User.SendNumeric(Numerics.RPL_LIST, c.Name, c.Users.Count);
				}
			}
			e.User.SendNumeric(Numerics.RPL_LISTEND, ":End of channel list.");
		}
	}
}