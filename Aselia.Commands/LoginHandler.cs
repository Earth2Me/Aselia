using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(CMD + " IDENTIFY ID", Authorizations.Unidentified)]
	public sealed class LoginHandler : ICommand
	{
		public const string CMD = "LOGIN";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.SendNumeric(Numerics.ERR_NEEDMOREPARAMS, CMD, ":Syntax: /login account password");
				return;
			}

			string account, password;
			if (e.Arguments.Length > 1)
			{
				account = e.Arguments[0].ToLower();
				password = e.Arguments[1];
			}
			else
			{
				account = e.User.Mask.Nickname.ToLower();
				password = e.Arguments[0];
			}

			byte[] hash = Password.Hash(password);
			if (hash == null)
			{
				e.User.SendNumeric(Numerics.ERR_COMPUTEHASH, CMD, ":Unable to compute password hash.");
				return;
			}

			if (e.Server.LogIn(e.User, account, hash))
			{
				e.User.SendNumeric(Numerics.RPL_IDENTIFIED, CMD, e.User.Mask.Account, ":You are now logged in as " + e.User.Mask.Account + ".");

				if (e.User.Level == Authorizations.NetworkOperator && !e.Server.NetworkOperators.Contains(e.User))
				{
					e.Server.NetworkOperators.Add(e.User);
				}
			}
			else
			{
				e.User.SendNumeric(Numerics.ERR_COMMANDSPECIFIC, CMD, ":Invalid credentials.");
			}
		}
	}
}