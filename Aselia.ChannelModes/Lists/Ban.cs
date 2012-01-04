using System.Collections.Generic;
using Aselia.Flags;
using Aselia.Modules;
using Aselia.Optimized;

namespace Aselia.ChannelModes.Lists
{
	[ChannelMode(Modes.b, ModeSyntax.Always, Authorizations.Identified, '@')]
	public sealed class Ban : ChannelList
	{
		public override List<HostMask> GetList(Channel channel)
		{
			return channel.Bans;
		}
	}
}