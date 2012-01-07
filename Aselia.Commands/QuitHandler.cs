using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(QuitHandler.CMD, Authorizations.None, ":{1} {0} :{2}")]
	public sealed class QuitHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "QUIT";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.User.Level >= Authorizations.Normal)
			{
				e.User.BroadcastInclusive(CMD, e.User.Mask, "Client quit.");
			}
			e.User.Dispose("Client quit.");
		}
	}
}