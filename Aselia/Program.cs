using Aselia.Common.Hotswap;

namespace Aselia
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			DomainManager domains = new DomainManager();
			domains.Reload();
			domains.Server.Run();
		}
	}
}