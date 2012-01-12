using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(PingHandler.CMD, Authorizations.Unregistered, "{0} :{3}")]
	public sealed class PingHandler : MarshalByRefObject, ICommand
	{
		public const string CMD = "PING";

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			Console.WriteLine("============");
			e.User.SendCommand("PONG", e.Server.Id, e.User.Mask);
		}
	}
}