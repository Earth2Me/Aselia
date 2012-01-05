using System.Collections.Generic;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes.Lists
{
	[ChannelMode(Modes.e, ModeSyntax.Always, Authorizations.Registered, '@')]
	public sealed class Except : ChannelList
	{
		public override List<HostMask> GetList(ChannelBase channel)
		{
			return channel.Exceptions;
		}
	}
}