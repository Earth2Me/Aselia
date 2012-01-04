using System;

namespace Aselia
{
	public class CancelableEventArgs : EventArgs
	{
		public bool IsCanceled { get; private set; }

		public void Cancel()
		{
			IsCanceled = true;
		}
	}
}