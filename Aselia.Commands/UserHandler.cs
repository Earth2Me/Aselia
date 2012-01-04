using System;
using Aselia.Optimized.UserHandlers;

namespace Aselia.UserCommands
{
	[Command(Commands.USER, Authorizations.Normal)]
	public sealed class UserHandler : MarshalByRefObject, ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}