﻿using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.v, ModeSyntax.Always, Authorizations.Registered, '%')]
	public class Voice : ChannelRank
	{
		public override char Prefix
		{
			get { return '+'; }
		}
	}
}