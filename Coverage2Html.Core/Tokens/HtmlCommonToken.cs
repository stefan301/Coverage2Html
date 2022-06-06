using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core.Tokens
{
	internal class HtmlCommonToken : CommonToken
	{
		public string TokenName { get; set; }
		public TokenCategory TokenCategory { get; set; }

		public HtmlCommonToken(int type) : base(type)
		{
		}

		public HtmlCommonToken(IToken oldToken) : base(oldToken)
		{
		}

		public HtmlCommonToken(int type, string text) : base(type, text)
		{
		}

		public HtmlCommonToken(Tuple<ITokenSource, ICharStream> source, int type, int channel, int start, int stop) : base(source, type, channel, start, stop)
		{
		}
	}
}
