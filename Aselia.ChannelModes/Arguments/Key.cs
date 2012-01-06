using System;
using Aselia.Common;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes
{
	[ChannelMode(Modes.k, ModeSyntax.OnAdd, Authorizations.Normal, '@')]
	public class Key : MarshalByRefObject, IChannelMode
	{
		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.Channel.Properties[this.GetType().Name] = e.Argument;
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.Channel.Properties.Remove(this.GetType().Name);
		}
	}
}