namespace Aselia.Common.Modules
{
	public interface IChannelMode
	{
		void AddHandler(object sender, ReceivedChannelModeEventArgs e);

		void RemoveHandler(object sender, ReceivedChannelModeEventArgs e);
	}
}