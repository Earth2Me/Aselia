﻿using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Flags
{
	[ChannelMode(Modes.p, ModeSyntax.Never, Authorizations.Registered, '@')]
	public class Preivate : ChannelFlag
	{
		public override string Flag
		{
			get { return GetType().Name; }
		}
	}
}