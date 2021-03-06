﻿using System;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes
{
	public abstract class ChannelRank : MarshalByRefObject, IChannelMode
	{
		public abstract char Prefix { get; }

		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			UserBase target = e.Channel.GetUser(e.Argument, e.User);
			if (target == null)
			{
				e.Cancel();
				return;
			}

			e.Channel.AddPrefix(target, Prefix);
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			UserBase target = e.Channel.GetUser(e.Argument, e.User);
			if (target == null)
			{
				e.Cancel();
				return;
			}

			e.Channel.RemovePrefix(target, Prefix);
		}
	}
}