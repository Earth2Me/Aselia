﻿using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.M, ModeSyntax.Never, Authorizations.Registered, '@')]
	public class NoActions : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}