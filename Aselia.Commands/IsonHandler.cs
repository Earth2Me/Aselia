using Aselia.Common;
using Aselia.Common.Core;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(IsonHandler.CMD, Authorizations.Unregistered)]
	public sealed class IsonHandler : ICommand
	{
		public const string CMD = "ISON";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
				return;
			}

			UserBase user = e.Server.GetUser(e.Arguments[0]);
			e.User.SendNumeric(Numerics.RPL_ISON, ":ison:", user == null ? "no such user" : user.Mask.Nickname);
		}
	}
}