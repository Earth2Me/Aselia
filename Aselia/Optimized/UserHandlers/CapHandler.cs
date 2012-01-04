using System.Linq;

namespace Aselia.Optimized.UserHandlers
{
	[Command(Commands.CAP, Authorizations.Connecting)]
	public sealed class CapHandler : ICommand
	{
		private const string SUPPORTED_STRING = "multi-prefix";
		private static readonly string[] SUPPORTED_ARRAY = SUPPORTED_STRING.Split(' ');

		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.NeedMoreParams();
				return;
			}

			e.User.WaitForCap = true;

			switch (e.Arguments[0].ToUpper())
			{
			case "LS":
				e.User.Send(Commands.LS, e.Server.Id, e.User.Mask.Nickname, SUPPORTED_STRING);
				break;

			case "REQ":
				if (e.User.Authorization != Authorizations.Connecting)
				{
					e.User.AlreadyRegistered();
				}
				if (e.Arguments.Length < 2)
				{
					e.User.NeedMoreParams();
					break;
				}
				for (int i = 1; i < e.Arguments.Length; i++)
				{
					string[] tok = e.Arguments[i].ToLower().Split(' ');
					for (int n = 0; n < tok.Length; n++)
					{
						if (SUPPORTED_ARRAY.Contains(tok[n]))
						{
							e.User.Send(Commands.ACK, e.Server.Id, e.User.Mask.Nickname, tok[n]);
							switch (tok[n])
							{
							case "multi-prefix":
								e.User.MultiPrefix = true;
								break;
							}
						}
						else
						{
							e.User.Send(Commands.NAK, e.Server.Id, e.User.Mask.Nickname, tok[n]);
						}
					}
				}
				break;

			case "END":
				if (e.User.Authorization == Authorizations.Connecting)
				{
					if (e.User.Mask.Nickname != "*" && e.User.Mask.Username != "*")
					{
						e.User.Authorization = Authorizations.Normal;
					}
					else
					{
						e.User.WaitForCap = false;
					}
				}
				else
				{
					e.User.AlreadyRegistered();
				}
				break;
			}
		}
	}
}