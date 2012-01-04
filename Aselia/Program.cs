using System.IO;
using Aselia.Configuration;

namespace Aselia
{
	public static class Program
	{
		private static Settings Settings;

		internal static void Main(string[] args)
		{
			Settings = GetSettings(args);
			try
			{
				using (Server server = new Server(Settings))
				{
					server.Run();
				}
			}
			finally
			{
				Settings.Save();
			}
		}

		private static Settings GetSettings(string[] args)
		{
			FileInfo file = new FileInfo(args.Length > 0 ? args[0] : "Configuration.xml");
			Settings settings;
			if (file.Exists)
			{
				settings = Settings.Load(file);
			}
			else
			{
				settings = new Settings();
				settings.SetDefaults();
				settings.File = file;
			}
			return settings;
		}
	}
}