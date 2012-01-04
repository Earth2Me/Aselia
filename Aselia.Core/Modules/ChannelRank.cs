using Aselia.Optimized;

namespace Aselia.Modules
{
	public abstract class ChannelRank : IChannelMode
	{
		public abstract char Prefix { get; }

		public void AddHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			User target = e.Channel.GetUser(e.Argument, e.User);
			if (target == null)
			{
				e.Cancel();
			}

			e.Channel.AddPrefix(target, Prefix);
		}

		public void RemoveHandler(object sender, ReceivedChannelModeEventArgs e)
		{
			User target = e.Channel.GetUser(e.Argument, e.User);
			if (target == null)
			{
				e.Cancel();
			}

			e.Channel.RemovePrefix(target, Prefix);
		}
	}
}