﻿using Aselia.Flags;
using Aselia.Modules;

namespace Aselia.ChannelModes
{
	[ChannelMode(Modes.m, ModeSyntax.Never, Authorizations.Identified, '@')]
	public class Moderated : IChannelMode
	{
		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.User.NotImplemented();
			e.Cancel();
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.User.NotImplemented();
			e.Cancel();
		}
	}
}