namespace Aselia.Common.Modules
{
	public interface IUserMode
	{
		void AddHandler(object sender, ReceivedUserModeEventArgs e);

		void RemoveHandler(object sender, ReceivedUserModeEventArgs e);
	}
}