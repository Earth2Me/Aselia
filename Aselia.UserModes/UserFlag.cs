using System;
using Aselia.Common.Modules;

namespace Aselia.UserModes
{
	public abstract class UserFlag : MarshalByRefObject, IUserMode
	{
		public abstract string Flag { get; }

		public void AddHandler(object sender, ReceivedUserModeEventArgs e)
		{
			if (!e.Target.SetSessionFlag(Flag))
			{
				e.Cancel();
			}
		}

		public void RemoveHandler(object sender, ReceivedUserModeEventArgs e)
		{
			if (!e.Target.ClearSessionFlag(Flag))
			{
				e.Cancel();
			}
		}
	}
}