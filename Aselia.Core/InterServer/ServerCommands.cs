namespace Aselia.Core.InterServer
{
	public enum ServerCommands : ushort
	{
		Void = 0,
		Reloading,
		ReloadingAcknowledge,
		JoinedLate,
	}
}