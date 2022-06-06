using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core.CoverageData
{
	public enum Position
	{
		Before,

		Within,

		After,
	}
	public class SourceLine : IComparable<SourceLine>
	{
		public uint LineStart { get; private set; } = 0;
		public uint ColumnStart { get; private set; } = 0;
		public uint LineEnd { get; private set; } = 0;
		public uint ColumnEnd { get; private set; } = 0;
		public uint Coverage { get; private set; } = 0;
		public string Type { get; private set; } = string.Empty;
		public string Signature { get; private set; } = string.Empty;

		public SourceLine( uint lineStart, uint columnStart, uint lineEnd, uint columnEnd, string type, string signature )
		{
			LineStart = lineStart;
			ColumnStart = columnStart;
			LineEnd = lineEnd;
			ColumnEnd = columnEnd == 0 ? uint.MaxValue : columnEnd;
			Type = type;
			Signature = signature;
		}

		internal void SetCoverageBit( int bit )
		{
			uint n = 1u << bit;

			Coverage |= n;
		}

		internal void AddCoverageBits( SourceLine other )
		{
			Coverage |= other.Coverage;
		}

		public Position HitTest( int line, int column )
		{
			if( line < LineStart )
				return Position.Before;

			if (line == LineStart && column < ColumnStart)
				return Position.Before;

			if (line > LineEnd)
				return Position.After;

			if (line == LineEnd && column > ColumnEnd)
				return Position.After;

			return Position.Within;
		}

		public int CompareTo(SourceLine other)
		{
			int n = this.LineStart.CompareTo(other.LineStart);
			if (n != 0)
				return n;

			n = this.ColumnStart.CompareTo(other.ColumnStart);
			if (n != 0)
				return n;

			n = this.LineEnd.CompareTo(other.LineEnd);
			if (n != 0)
				return n;

			n = this.ColumnEnd.CompareTo(other.ColumnEnd);
			if (n != 0)
				return n;

			return 0;
		}
	}
}
