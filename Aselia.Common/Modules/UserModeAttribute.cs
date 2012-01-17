using System;
using Aselia.Common.Flags;

namespace Aselia.Common.Modules
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class UserModeAttribute : Attribute
	{
		public Modes Mode { get; private set; }

		public ModeSyntax Syntax { get; private set; }

		public Authorizations Level { get; private set; }

		public UserModeAttribute(Modes mode, ModeSyntax syntax, Authorizations level)
			: base()
		{
			Mode = mode;
			Syntax = syntax;
			Level = level;
		}
	}
}