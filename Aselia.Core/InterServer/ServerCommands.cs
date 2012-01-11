namespace Aselia.Core.InterServer
{
	public enum ServerCommands : byte
	{
		Void = 0,
		Reloading,
		JoinedLate,
		Dispose,
		ToChannel,
		ToUser,
		CacheRequest,
		Cache,
	}
}