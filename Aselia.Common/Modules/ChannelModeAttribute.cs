using System;
using Aselia.Common.Flags;

namespace Aselia.Common.Modules
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class ChannelModeAttribute : Attribute
	{
		public Modes Mode { get; private set; }

		public ModeSyntax Syntax { get; private set; }

		public Authorizations Level { get; private set; }

		public char? Prefix { get; private set; }

		public ChannelModeAttribute(Modes mode, ModeSyntax syntax, Authorizations level, char prefix = (char)0)
			: base()
		{
			Mode = mode;
			Syntax = syntax;
			Level = level;
			Prefix = prefix == 0 ? (char?)null : prefix;
		}
	}
}