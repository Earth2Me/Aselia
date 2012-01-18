using System;
using System.Linq;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(CapHandler.CMD, Authorizations.Connecting, ":{1} {0} {2} {3} {4}")]
	public sealed class CapHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "CAP";
		private const string SUPPORTED_STRING = "sasl multi-prefix";
		private static readonly string[] SUPPORTED_ARRAY = SUPPORTED_STRING.Split(' ');

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			if (e.User.Level != Authorizations.Connecting)
			{
				e.User.ErrorAlreadyRegistered(CMD);
			}

			e.User.SetSessionFlag("WaitForCap");

			switch (e.Arguments[0].ToUpper())
			{
			case "LS":
				e.User.SendCommand(CMD, e.Server.Id, e.User.Mask.Nickname, "LS", SUPPORTED_STRING);
				break;

			case "REQ":
				if (e.Arguments.Length < 2)
				{
					e.User.ErrorNeedMoreParams(CMD);
					break;
				}
				for (int i = 1; i < e.Arguments.Length; i++)
				{
					string[] tok = e.Arguments[i].ToLower().Split(' ');
					for (int n = 0; n < tok.Length; n++)
					{
						if (SUPPORTED_ARRAY.Contains(tok[n]))
						{
							e.User.SendCommand(CMD, e.Server.Id, e.User.Mask.Nickname, "ACK", ":" + tok[n]);
							switch (tok[n])
							{
							case "multi-prefix":
								e.User.SetSessionFlag("MultiPrefix");
								break;

							case "sasl":
								e.User.SetSessionFlag("SaslRequested");
								break;
							}
						}
						else
						{
							e.User.SendCommand(CMD, e.Server.Id, e.User.Mask.Nickname, "NAK", ":" + tok[n]);
						}
					}
				}
				break;

			case "END":
				if (e.User.Level == Authorizations.Connecting || e.User.ClearSessionFlag("SaslWaitForCapEnd"))
				{
					e.User.ClearSessionFlag("WaitForCap");
					if (e.User.HasSessionFlag("PassedUser") && e.User.HasSessionFlag("PassedNick"))
					{
						e.User.ClearSessionFlag("PassedUser");
						e.User.ClearSessionFlag("PassedNick");
						e.User.OnConnected();
					}
				}
				else
				{
					e.User.ErrorAlreadyRegistered(CMD);
				}
				break;
			}
		}
	}
}