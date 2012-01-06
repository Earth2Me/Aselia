using System;

namespace Aselia.Common.Modules
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class CommandAttribute : Attribute
	{
		public string Command { get; private set; }

		public Authorizations Level { get; private set; }

		public string Format { get; private set; }

		public CommandAttribute(string command, Authorizations level, string format = "")
			: base()
		{
			Command = command.ToUpper();
			Level = level;
			Format = format;
		}
	}
}