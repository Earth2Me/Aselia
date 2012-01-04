using System;
using Aselia.Optimized.UserHandlers;

namespace Aselia.UserCommands
{
	[Command(Commands.JOIN, Authorizations.Normal)]
	public sealed class JoinHandler : MarshalByRefObject, ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}