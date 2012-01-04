using System;

namespace Aselia.Optimized.UserHandlers
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class CommandAttribute : Attribute
	{
		public Commands Command { get; private set; }

		public Authorizations Level { get; private set; }

		public CommandAttribute(Commands command, Authorizations level)
			: base()
		{
			Command = command;
			level = level;
		}
	}
}