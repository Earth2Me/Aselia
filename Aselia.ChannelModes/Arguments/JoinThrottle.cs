using System;
using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes
{
	[ChannelMode(Modes.j, ModeSyntax.OnAdd, Authorizations.Unregistered, '@')]
	public class JoinThrottle : MarshalByRefObject, IChannelMode
	{
		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			uint value;
			if (uint.TryParse(e.Argument, out value))
			{
				e.Channel.Properties[this.GetType().Name] = value;
			}
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.Channel.Properties.Remove(this.GetType().Name);
		}
	}
}