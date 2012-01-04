using System;
using Aselia.Optimized.UserHandlers;

namespace Aselia.UserCommands
{
	[Command(Commands.MODE, Authorizations.Normal)]
	public sealed class ModeHandler : MarshalByRefObject, ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}