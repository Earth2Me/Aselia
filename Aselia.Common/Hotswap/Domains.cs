using System;

namespace Aselia.Common.Hotswap
{
	[Flags]
	public enum Domains : byte
	{
		ChannelModes,
		Core,
		UserCommands,
		UserModes,
	}
}