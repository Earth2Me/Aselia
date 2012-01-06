using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(HotswapHandler.CMD, Authorizations.NetworkOperator)]
	public sealed class HotswapHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "HOTSWAP";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.SendCommand("NOTICE", e.Server.Id, e.User.Mask.Nickname, "Hotswapping core and all modules.");
			e.Server.Domains.Reload();
		}
	}
}