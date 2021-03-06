﻿namespace Aselia.Common
{
	public enum Numerics : ushort
	{
		VOID = 000,

		// RFC Compliant

		RPL_WELCOME = 001,
		RPL_YOURHOST = 002,
		RPL_CREATED = 003,
		RPL_MYINFO = 004,
		RPL_ISUPPORT = 005,

		RPL_REDIRECT = 010,

		RPL_TRACELINK = 200,
		RPL_TRACECONNECTING = 201,
		RPL_TRACEHANDSHAKE = 202,
		RPL_TRACEUNKNOWN = 203,
		RPL_TRACEOPERATOR = 204,
		RPL_TRACEUSER = 205,
		RPL_TRACESERVER = 206,
		RPL_TRACESERVICE = 207,
		RPL_TRACENEWTYPE = 208,
		RPL_TRACECLASS = 209,
		RPL_TRACERECONNECT = 210,
		RPL_STATSLINKINFO = 211,
		RPL_STATSCOMMANDS = 212,
		RPL_STATSCLINE = 213,
		RPL_STATSNLINE = 214,
		RPL_STATSILINE = 215,
		RPL_STATSKLINE = 216,
		RPL_STATSQLINE = 217,
		RPL_STATSYLINE = 218,
		RPL_ENDOFSTATS = 219,

		RPL_UMODEIS = 221,

		RPL_SERVICEINFO = 231,
		RPL_ENDOFSERVICES = 232,
		RPL_SERVICE = 233,
		RPL_SERVLIST = 234,
		RPL_SERVLISTEND = 235,

		RPL_STATSVLINE = 240,
		RPL_STATSLLINE = 241,
		RPL_STATSUPTIME = 242,
		RPL_STATSOLINE = 243,
		RPL_STATSHLINE = 244,

		RPL_STATSPING = 246,
		RPL_STATSBLINE = 247,

		RPL_STATSDLINE = 250,
		RPL_LUSERCLIENT = 251,
		RPL_LUSEROP = 252,
		RPL_LUSERUNKNOWN = 253,
		RPL_LUSERCHANNELS = 254,
		RPL_LUSERME = 255,
		RPL_ADMINME = 256,
		RPL_ADMINLOC1 = 257,
		RPL_ADMINLOC2 = 258,
		RPL_ADMINEMAIL = 259,

		RPL_TRACELOG = 261,
		RPL_TRACEND = 262,
		RPL_TRYAGAIN = 263,

		RPL_NONE = 300,
		RPL_AWAY = 301,
		RPL_USERHOST = 302,
		RPL_ISON = 303,
		RPL_TEXT = 304,
		RPL_UNAWAY = 305,
		RPL_NOAWAY = 306,

		RPL_WHOISUSER = 311,
		RPL_WHOISSERVER = 312,
		RPL_WHOISOPERATOR = 313,
		RPL_WHOWASUSER = 314,
		RPL_ENDOFWHO = 315,
		RPL_WHOISCHANOP = 316,
		RPL_WHOISIDLE = 317,
		RPL_ENDOFWHOIS = 318,
		RPL_WHOISCHANNELS = 319,

		RPL_LISTSTART = 321,
		RPL_LIST = 322,
		RPL_LISTEND = 323,
		RPL_CHANNELMODEIS = 324,
		RPL_UNIQOPIS = 325,

		RPL_NOTOPIC = 331,
		RPL_TOPIC = 332,

		RPL_INVITING = 341,
		RPL_SUMMONING = 342,

		RPL_INVITELIST = 346,
		RPL_ENDOFINVITELIST = 347,
		RPL_EXCEPTLIST = 348,
		RPL_ENDOFEXCEPTLIST = 349,

		RPL_VERSION = 351,
		RPL_WHOREPLY = 352,
		RPL_NAMREPLY = 353,

		RPL_KILLDONE = 361,
		RPL_CLOSING = 362,
		RPL_CLOSEEND = 363,
		RPL_LINKS = 364,
		RPL_ENDOFLINKS = 365,
		RPL_ENDOFNAMES = 366,

		RPL_BANLIST = 367,
		RPL_ENDOFBANLIST = 368,
		RPL_ENDOFWHOWAS = 369,

		RPL_INFO = 371,
		RPL_MOTD = 372,
		RPL_INFOSTART = 373,
		RPL_ENDOFINFO = 374,
		RPL_MOTDSTART = 375,
		RPL_ENDOFMOTD = 376,

		RPL_TIME = 391,
		RPL_USERSSTART = 392,
		RPL_USERS = 393,
		RPL_ENDOFUSERS = 394,
		RPL_NOUSERS = 395,

		ERR_UNKNOWNERROR = 400,
		ERR_NOSUCHNICK = 401,
		ERR_NOSUCHSERVER = 402,
		ERR_NOSUCHCHANNEL = 403,
		ERR_CANNOTSENDTOCHAN = 403,
		ERR_TOOMANYCHANNELS = 405,
		ERR_WASNOSUCHNICK = 406,
		ERR_TOOMANYTARGETS = 407,
		ERR_NOSUCHSERVICE = 408,
		ERR_NOORIGIN = 409,

		ERR_NORECIPIENT = 411,
		ERR_NOTEXTTOSEND = 412,
		ERR_NOTOPLEVEL = 413,
		ERR_WILDTOPLEVEL = 414,
		ERR_BADMASK = 415,

		ERR_UNKNOWNCOMMAND = 421,
		ERR_NOMOTD = 422,
		ERR_NOADMININFO = 423,
		ERR_FILEERROR = 424,

		ERR_NONICKNAMEGIVEN = 431,
		ERR_ERRONEUSNICKNAME = 432,
		ERR_NICKNAMEINUSE = 433,

		ERR_NICKCOLLISION = 436,
		ERR_UNAVAILRESOURCE = 437,
		ERR_USERNOTINCHANNEL = 441,
		ERR_NOTONCHANNEL = 442,
		ERR_USERONCHANNEL = 443,
		ERR_NOLOGIN = 444,
		ERR_SUMMONDISABLED = 445,
		ERR_USERSDISABLED = 446,

		ERR_NOTREGISTERED = 451,

		ERR_NEEDMOREPARAMS = 461,
		ERR_ALREADYREGISTERED = 462,
		ERR_NOPERMFORHOST = 463,
		ERR_PASSWDMISMATCH = 464,
		ERR_YOUREBANNEDCREEP = 465,
		ERR_YOUWILLBEBANNED = 466,
		ERR_KEYSET = 467,

		ERR_CHANNELISFULL = 471,
		ERR_UNKNOWNMODE = 472,
		ERR_INVITEONLYCHAN = 473,
		ERR_BANNEDFROMCHAN = 474,
		ERR_BADCHANNELKEY = 475,
		ERR_BADCHANMASK = 476,
		ERR_NOCHANMODES = 477,
		ERR_BANLISTFULL = 478,

		ERR_NOPRIVILEGES = 481,
		ERR_CHANOPPRIVSNEEDED = 482,
		ERR_CANTKILLSERVER = 483,
		ERR_RESTRICTED = 484,
		ERR_UNIQOPRIVSNEEDED = 485,

		ERR_NOOPERHOST = 491,
		ERR_NOSERVICEHOST = 492,

		ERR_UMODEUNKNOWNFLAG = 501,
		ERR_USERSDONTMATCH = 502,

		// Extended

		RPL_WHOISACCOUNT = 330,

		RPL_HELPSTART = 704,
		RPL_HELPTXT = 705,
		RPL_ENDOFHELP = 706,

		// Aselia

		RPL_MISCELLANEOUS = 800,
		RPL_REGISTERED = 801,
		RPL_IDENTIFIED = 802,
		RPL_FLAGSET = 803,
		RPL_FLAGPROPCLEARED = 804,
		RPL_PROPERTYSET = 805,
		RPL_METAFLAGS = 806,
		RPL_METAPROPERTY = 807,
		RPL_COMMITSTART = 808,
		RPL_COMMITSUCCESS = 809,
		RPL_REHASHING = 810,
		RPL_PROMOTED = 811,
		RPL_DEMOTED = 812,
		RPL_ACCOUNTDROPPED = 813,
		RPL_CHANNELDROPPED = 814,

		ERR_COMMANDSPECIFIC = 850,
		ERR_COMPUTEHASH = 851,
		ERR_NOSUCHACCOUNT = 853,
		ERR_SYNTAX = 854,
		ERR_CHANNELEXISTS = 855,
		ERR_CONCURRENCY = 856,
		ERR_COMMITFAIL = 857,
		ERR_COMMANDDISABLED = 858,

		// SASL

		RPL_SASLLOGGEDINAS = 900, // Sent first
		RPL_SASLSUCCESS = 903, // Sent second

		ERR_SASLABORTED = 904, // Either client aborted or server could not connect to services.
		ERR_SASLFAILED = 905, // Invalid credentials.
		ERR_SASLINTERRUPTED = 906, // Client completed registration without finishing SASL authentication.
		ERR_SASLALREADYAUTHED = 907, // SASL authentication has already been completed successfully.
	}
}