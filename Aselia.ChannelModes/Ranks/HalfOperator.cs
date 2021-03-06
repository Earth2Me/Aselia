﻿using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Ranks
{
	[ChannelMode(Modes.h, ModeSyntax.Always, Authorizations.Unregistered, '@')]
	public class HalfOperator : ChannelRank
	{
		public override char Prefix
		{
			get { return '%'; }
		}
	}
}