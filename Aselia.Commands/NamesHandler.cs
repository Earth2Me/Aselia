using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(NamesHandler.CMD, Authorizations.Normal)]
	public sealed class NamesHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "NAMES";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.ErrorNeedMoreParams(CMD);
			}

			e.User.Names(e.Arguments[0]);
		}
	}
}