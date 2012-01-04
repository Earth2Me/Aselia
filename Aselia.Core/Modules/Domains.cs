using System;

namespace Aselia.Modules
{
	[Flags]
	public enum Domains : byte
	{
		UserCommands,
		ChannelModes,
	}
}