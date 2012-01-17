using System.Text.RegularExpressions;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(RegisterHandler.CMD, Authorizations.Unregistered)]
	public sealed class RegisterHandler : ICommand
	{
		public const string CMD = "REGISTER";
		private static readonly Regex Email;

		static RegisterHandler()
		{
			Email = new Regex(@"^[a-z0-9_\-.]+@[a-z0-9_\-.]+\.[a-z]{2,4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		}

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 2)
			{
				e.User.SendNumeric(Numerics.ERR_NEEDMOREPARAMS, CMD, ":Syntax: /register <password> <e-mail>");
				return;
			}

			if (e.Arguments[0].Length < 7)
			{
				e.User.SendNumeric(Numerics.ERR_COMMANDSPECIFIC, CMD, ":Your password must be at least seven characters in length.");
				return;
			}

			if (!Email.IsMatch(e.Arguments[1]))
			{
				e.User.SendNumeric(Numerics.ERR_COMMANDSPECIFIC, CMD, ":The specified e-mail address is invalid.");
				return;
			}

			byte[] password = Password.Hash(e.Arguments[0]);
			if (password == null)
			{
				e.User.SendNumeric(Numerics.ERR_COMPUTEHASH, CMD, ":Unable to computer password hash.");
				return;
			}

			if (!e.User.Register(password, e.Arguments[1]))
			{
				e.User.SendNumeric(Numerics.ERR_ALREADYREGISTERED, ":You are already registered.");
				return;
			}

			if (e.User.Level == Authorizations.NetworkOperator && !e.Server.NetworkOperators.Contains(e.User))
			{
				e.Server.NetworkOperators.Add(e.User);
			}
		}
	}
}