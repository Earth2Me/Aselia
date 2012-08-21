using System;
using Aselia.Common;
using Aselia.Common.Modules;

namespace Aselia.UserCommands
{
	[Command("SUMMON", Authorizations.Unregistered)]
	public sealed class SummonHandler : ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			// TODO: Implement SUMMON.
			e.User.SendNumeric(Numerics.ERR_SUMMONDISABLED, e.Command, ":SUMMON is disabled on this server.");
		}
	}
}

