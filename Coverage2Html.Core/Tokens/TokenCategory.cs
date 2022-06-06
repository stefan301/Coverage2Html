using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core
{
	public enum TokenCategory
	{
		Undefined,
		Whitespace,
		Newline,
		Directive,
		Comment,
		Literal,
		String,
		Keyword,
		Type,
		Identifier,
		Operator
	}
}
