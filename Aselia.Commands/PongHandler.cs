using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(PongHandler.CMD, Authorizations.Unregistered, ":{1} {0} {2} :{3}")]
	public sealed class PongHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "PONG";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// Intentionally blank.
		}
	}
}