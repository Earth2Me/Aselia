using System;

namespace Aselia.Common.Hotswap
{
	[Flags]
	public enum Domains : byte
	{
		Core,
		UserCommands,
		ChannelModes,
	}
}