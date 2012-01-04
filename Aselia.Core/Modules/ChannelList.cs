using System.Collections.Generic;
using Aselia.Flags;
using Aselia.Optimized;

namespace Aselia.Modules
{
	public abstract class ChannelList : IChannelMode
	{
		public abstract List<HostMask> GetList(Channel channel);

		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			List<HostMask> list = GetList(e.Channel);

			if (list.Count > e.Server.Settings.MaximumListLength
				&& (!e.Channel.Options.HasFlag(ChannelModes.LargeLists)
				|| list.Count > e.Server.Settings.MaximumLargeListLength))
			{
				e.User.SendNumeric(Commands.ERR_BANLISTFULL, "That list is full.  Contact a network operator to override this.");
				e.Cancel();
				return;
			}

			HostMask mask = HostMask.Parse(e.Argument);

			if ((mask.Nickname == null || mask.Username == null || mask.Hostname == null) && mask.Account == null)
			{
				e.User.SendNumeric(Commands.ERR_BADMASK, "List entries must be fully qualified hostmasks.");
				e.Cancel();
				return;
			}

			if (list.Contains(mask))
			{
				e.Cancel();
				return;
			}

			list.Add(mask);
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			List<HostMask> list = GetList(e.Channel);
			HostMask mask = HostMask.Parse(e.Argument);

			if (!list.Remove(mask))
			{
				e.Cancel();
			}
		}
	}
}