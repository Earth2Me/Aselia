using System;

namespace Aselia.Common.Core.Configuration
{
	[Serializable]
	public abstract class Line<T> : MarshalByRefObject
	{
		public T Ban { get; set; }

		public DateTime? Set { get; set; }

		public DateTime? Expires { get; set; }

		public string Reason { get; set; }

		public string By { get; set; }

		public bool Automated { get; set; }
	}
}