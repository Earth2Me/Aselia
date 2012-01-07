using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(PongHandler.CMD, Authorizations.Normal, "{0}")]
	public sealed class PongHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "PONG";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// Intentionally blank.
		}
	}
}