using System;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes
{
	[ChannelMode(Modes.f, ModeSyntax.OnAdd, Authorizations.Unregistered, '@')]
	public class Forward : MarshalByRefObject, IChannelMode
	{
		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			ChannelBase channel = e.Server.GetChannel(e.Argument);
			if (channel != null && (e.User.IsProtect(channel) || channel.HasFlag("FreeTarget")))
			{
				e.Channel.Properties[this.GetType().Name] = e.Argument;
			}
			else
			{
				e.User.SendNumeric(Numerics.ERR_RESTRICTED, "MODE", ":You must be +a in the target channel in order to forward to it, unless it is a free target.");
				e.Cancel();
			}
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.Channel.Properties.Remove(this.GetType().Name);
		}
	}
}