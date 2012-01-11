using System;

namespace Aselia.Common.Modules
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class CommandAttribute : Attribute
	{
		public string[] Commands { get; private set; }

		public Authorizations Level { get; private set; }

		public string Format { get; private set; }

		public CommandAttribute(string commands, Authorizations level, string format = "")
			: base()
		{
			Commands = commands.ToUpper().Split(' ');
			Level = level;
			Format = format;
		}
	}
}