using System;
using Aselia.Optimized.UserHandlers;

namespace Aselia.UserCommands
{
	[Command(Commands.NAMES, Authorizations.Normal)]
	public sealed class NamesHandler : MarshalByRefObject, ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}