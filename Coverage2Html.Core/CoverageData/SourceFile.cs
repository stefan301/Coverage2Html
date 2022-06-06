using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core.CoverageData
{
	public class SourceFile
	{
		public string Path { get; private set; } = String.Empty;

		public SortedSet<SourceLine> Lines { get; private set; } = new SortedSet<SourceLine>();
		public SourceFile(string path)
		{
			Path = path;
		}

		internal void AddLine(SourceLine line)
		{
			if (Lines.TryGetValue(line, out SourceLine existingline))
			{
				existingline.AddCoverageBits(line);
			}
			else
			{
				Lines.Add(line);
			}
		}
	}
}
