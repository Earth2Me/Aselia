using System;
using Aselia.Common.Modules;

namespace Aselia.ChannelModes
{
	public abstract class ChannelArgument : MarshalByRefObject, IChannelMode
	{
		public abstract string Key { get; }

		public abstract object Parse(string argument);

		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			e.Channel.Properties[Key] = Parse(e.Argument);
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			if (!e.Channel.Properties.Remove(Key))
			{
				e.Cancel();
			}
		}
	}
}