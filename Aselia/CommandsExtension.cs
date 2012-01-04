using System;

namespace Aselia
{
	public static class CommandsExtension
	{
		public static string GetToken(this Commands command)
		{
			ushort cmd = (ushort)command;

			if (cmd == 0)
			{
				return "000";
			}
			else if (cmd < 10)
			{
				return "00" + cmd;
			}
			else if (cmd < 100)
			{
				return "0" + cmd;
			}
			else if (cmd < 1000)
			{
				return cmd.ToString();
			}
			else
			{
				return Enum.GetName(typeof(Commands), command);
			}
		}
	}
}