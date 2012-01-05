using System;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes
{
	public abstract class ChannelFlag : MarshalByRefObject, IChannelMode
	{
		public abstract string Flag { get; }

		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			if (!e.Channel.SetFlag(Flag))
			{
				e.Cancel();
			}
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			if (!e.Channel.ClearFlag(Flag))
			{
				e.Cancel();
			}
		}
	}
}