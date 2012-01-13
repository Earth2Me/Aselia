namespace Aselia.Core.InterServer
{
	public enum ServerCommands : byte
	{
		Void = 0,
		Received,
		Reloading,
		JoinedLate,
		Dispose,
		ToChannel,
		ToUser,
		CacheRequest,
		Cache,
	}
}