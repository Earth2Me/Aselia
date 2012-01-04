namespace Aselia.Modules
{
	public interface ICommand
	{
		void Handler(object sender, ReceivedCommandEventArgs e);
	}
}