using System;
using System.Text;
using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(AuthenticateCommand.CMD, Authorizations.Connecting, "{0} {2}")]
	public sealed class AuthenticateCommand : ICommand
	{
		public const string CMD = "AUTHENTICATE";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			if (!e.User.ClearSessionFlag("SaslRequested"))
			{
				e.User.SendNumeric(Numerics.ERR_COMMANDSPECIFIC, ":CAP AUTHENTICATE can only be issued after requesting \"sasl\" via CAP REQ.");
				return;
			}

			switch (e.Arguments[0])
			{
			case "*":
				e.User.SendNumeric(Numerics.ERR_SASLABORTED, ":Client aborted SASL authentication.");
				break;

			case "-":
				e.User.SendNumeric(Numerics.ERR_SASLABORTED, ":Client sent blank SASL authentication code.");
				break;

			case "PLAIN":
				if (e.User.HasSessionFlag("SaslPlain"))
				{
					e.User.SendNumeric(Numerics.ERR_SASLABORTED, ":You have already chosen an authentication method.");
				}
				else
				{
					e.User.SetSessionFlag("SaslPlain");
					e.User.SendCommand(CMD, e.User.Server.Id, e.User.Mask.Nickname, "AUTHENTICATE +");
				}
				break;

			default:
				if (e.User.ClearSessionFlag("SaslPlain"))
				{
					AuthenticatePlain(e.User, e.Arguments[0]);
				}
				else
				{
					e.User.SendNumeric(Numerics.ERR_SASLABORTED, ":That SASL authentication method is not supported.  Use PLAIN instead.");
				}
				break;
			}
		}

		private void AuthenticatePlain(UserBase user, string base64)
		{
			string[] args;
			try
			{
				args = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(base64)).Split('\0');
			}
			catch
			{
				user.SendNumeric(Numerics.ERR_SASLABORTED, ":Unable to process SASL authentication string as base-64-encoded ASCII.");
				return;
			}

			if (args.Length != 3)
			{
				user.SendNumeric(Numerics.ERR_SASLABORTED, ":Expected three parameters in the base-64-encoded ASCII string for SASL authentication, separated by null-terminators.");
				return;
			}

			if (args[0] != user.Mask.Nickname)
			{
				user.SendNumeric(Numerics.ERR_SASLFAILED, ":Nicknames do not match.");
			}
			else if (user.Server.LogIn(user, args[1], Password.Hash(args[2])))
			{
				user.SendNumeric(Numerics.RPL_SASLLOGGEDINAS, user.Mask, user.Mask.Account, ":You are now logged in as", user.Mask.Account + ".");
				user.SendNumeric(Numerics.RPL_SASLSUCCESS, ":SAS authentication successful.");
				user.SetSessionFlag("Sasl");
				user.SetSessionFlag("SaslWaitForCapEnd");
				return;
			}

			user.SetSessionFlag("SaslPlain");
		}
	}
}