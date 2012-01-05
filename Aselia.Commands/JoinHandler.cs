using System;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command(Commands.JOIN, Authorizations.Normal)]
	public sealed class JoinHandler : MarshalByRefObject, ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			e.User.NotImplemented();
		}
	}
}