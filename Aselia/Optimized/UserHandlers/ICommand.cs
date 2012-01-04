namespace Aselia.Optimized.UserHandlers
{
	public interface ICommand
	{
		void Handler(object sender, ReceivedCommandEventArgs e);
	}
}