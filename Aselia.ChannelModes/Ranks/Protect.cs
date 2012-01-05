﻿using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.a, ModeSyntax.Always, Authorizations.Registered, '~')]
	public class Protect : ChannelRank
	{
		public override char Prefix
		{
			get { return '&'; }
		}
	}
}