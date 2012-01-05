using System;

namespace Aselia.Common.Modules
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
			Level = level;
		}
	}
}