namespace Aselia.Optimized.UserHandlers
{
	[Command(Commands.NICK, Authorizations.Connecting)]
	public class NickHandler : ICommand
	{
		public void Handler(object sender, ReceivedCommandEventArgs e)
		{
			if (e.Arguments.Length < 1)
			{
				e.User.NeedMoreParams();
				return;
			}
			string nickname = e.Arguments[0];

			if (e.User.Authorization != Authorizations.Connecting)
			{
				e.User.BroadcastInclusive(Commands.NICK, e.User, nickname);
			}

			e.User.Mask.Nickname = nickname;
		}
	}
}