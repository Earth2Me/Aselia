using System;

namespace Aselia.Flags
{
	[Flags]
	public enum ChannelModes : ulong
	{
		Protect = Modes.a,
		Ban = Modes.b,
		NoColor = Modes.c,
		Exception = Modes.e,
		Forward = Modes.f,
		FreeInvite = Modes.g,
		HalfOperator = Modes.h,
		InviteOnly = Modes.i,
		JoinThrottle = Modes.j,
		Key = Modes.k,
		Limit = Modes.l,
		Moderated = Modes.m,
		NoExternal = Modes.n,
		Operator = Modes.o,
		Private = Modes.p,
		Quiet = Modes.q,
		RegisteredOnly = Modes.r,
		Secret = Modes.s,
		LockTopic = Modes.t,
		Arena = Modes.u,
		Voice = Modes.v,
		IrcOp = Modes.x,
		OpModerated = Modes.z,
		NoCtcps = Modes.C,
		FreeTarget = Modes.F,
		InviteExcept = Modes.I,
		LargeLists = Modes.L,
		NoActions = Modes.M,
		Owner = Modes.O,
		Permanent = Modes.P,
		DisableForward = Modes.Q,
		Service = Modes.S,
	}
}