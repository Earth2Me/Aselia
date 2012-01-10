namespace Aselia.Core.InterServer
{
	public static class Rfc
	{
		public static string ToCommand(byte numeric)
		{
			switch (numeric)
			{
			case 1:
				return "CAP";

			case 2:
				return "USER";

			case 3:
				return "NICK";

			case 4:
				return "PING";

			case 5:
				return "PONG";

			case 6:
				return "MODE";

			case 7:
				return "KICK";

			case 8:
				return "PRIVMSG";

			case 9:
				return "NOTICE";

			case 10:
				return "JOIN";

			case 11:
				return "PART";

			case 12:
				return "QUIT";

			default:
				return null;
			}
		}

		public static byte FromCommand(string command)
		{
			switch (command)
			{
			case "CAP":
				return 0;

			case "USER":
				return 1;

			case "NICK":
				return 2;

			case "PING":
				return 3;

			case "PONG":
				return 4;

			case "MODE":
				return 5;

			case "KICK":
				return 6;

			case "PRIVMSG":
				return 7;

			case "NOTICE":
				return 8;

			case "JOIN":
				return 9;

			case "PART":
				return 10;

			case "QUIT":
				return 11;

			default:
				return 0;
			}
		}
	}
}