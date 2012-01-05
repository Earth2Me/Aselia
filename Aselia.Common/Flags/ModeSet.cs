using System;
using System.Collections.Generic;
using System.Linq;

namespace Aselia.Common.Flags
{
	[Serializable]
	public class ModeSet : MarshalByRefObject, IEquatable<ModeSet>, IEquatable<Modes>
	{
		[NonSerialized]
		private string _Text;
		[NonSerialized]
		private Modes Cached;

		public Modes Raw { get; set; }

		public Dictionary<Modes, object> Arguments { get; set; }

		public string Text
		{
			get
			{
				if (_Text == null || Raw != Cached)
				{
					_Text = Raw.ToFlagString();
				}
				return _Text;
			}
		}

		public ModeSet()
		{
		}

		public ModeSet(Modes raw, Dictionary<Modes, object> arguments)
		{
			Raw = raw;
			Arguments = arguments;
		}

		public bool Has(Modes mode)
		{
			return Raw.HasFlag(mode);
		}

		public bool Has(char mode)
		{
			return Text.Contains(mode);
		}

		public override int GetHashCode()
		{
			return Raw.GetHashCode();
		}

		public override string ToString()
		{
			return Text;
		}

		public bool Equals(Modes other)
		{
			return Raw == other;
		}

		public bool Equals(ModeSet other)
		{
			return Raw == other.Raw;
		}

		public static bool operator ==(ModeSet a, ModeSet b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ModeSet a, ModeSet b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			if (obj is ModeSet)
			{
				return Equals((ModeSet)obj);
			}
			else if (obj is Modes)
			{
				return Equals((Modes)obj);
			}
			else
			{
				return false;
			}
		}
	}
}