using System;
using System.Collections.Generic;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes
{
	public abstract class ChannelList : MarshalByRefObject, IChannelMode
	{
		public abstract List<HostMask> GetList(ChannelBase channel);

		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			List<HostMask> list = GetList(e.Channel);

			if (list.Count > (int)e.Server.Settings["MaximumListLength"]
				&& (!e.Channel.HasFlag("LargeLists")
				|| list.Count > (int)e.Server.Settings["MaximumLargeListLength"]))
			{
				e.User.SendNumeric(Numerics.ERR_BANLISTFULL, "That list is full.  Contact a network operator to override this.");
				e.Cancel();
				return;
			}

			HostMask mask = HostMask.Parse(e.Argument);

			if ((mask.Nickname == null || mask.Username == null || mask.Hostname == null) && mask.Account == null)
			{
				e.User.SendNumeric(Numerics.ERR_BADMASK, "List entries must be fully qualified hostmasks.");
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