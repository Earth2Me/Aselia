using System;

namespace Aselia.Common
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