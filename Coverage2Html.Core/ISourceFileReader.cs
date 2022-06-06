using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core
{
	public interface ISourceFileReader
	{
		CommonTokenStream ReadSourceFile(string path, TextWriter output, TextWriter errorOutput);
	}
}
