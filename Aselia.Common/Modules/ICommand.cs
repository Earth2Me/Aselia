namespace Aselia.Common.Modules
{
	public interface ICommand
	{
		void Handler(object sender, ReceivedCommandEventArgs e);
	}
}