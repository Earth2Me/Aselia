using System.Collections.Generic;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Lists
{
	[ChannelMode(Modes.b, ModeSyntax.Always, Authorizations.Registered, '@')]
	public sealed class Ban : ChannelList
	{
		public override List<HostMask> GetList(ChannelBase channel)
		{
			return channel.Bans;
		}
	}
}