using Antlr4.Runtime;
using Coverage2Html.Core.CoverageData;
using Coverage2Html.Core.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core.Html
{
	public class HtmlGenerator
	{
                public static void WriteHtml(CommonTokenStream tokens, SourceFile sourceFile, string outputPath, string relativeCssPath)
                {
                        var html = new StringBuilder();

                        html.Append($@"<!DOCTYPE html>
<html>
<head>
<link rel='stylesheet' href='{relativeCssPath}'>
</head>
<title>{outputPath}</title>
<body>
<pre>
<code>
");
                        int? lastLine = null;
                        SourceLine lastCoverageLine = null;
                        SourceLine currentCoverageLine = null;

                        var lines = sourceFile.Lines.GetEnumerator();
                        if (lines.MoveNext())
                                currentCoverageLine = lines.Current;

                        foreach (HtmlCommonToken token in tokens.GetTokens())
                        {
                                if (lastCoverageLine != null)
                                {
                                        if (token.TokenCategory == TokenCategory.Newline && token.Line >= lastCoverageLine.LineEnd ||
                                                lastCoverageLine.HitTest(token.Line, token.Column) == Position.After)
                                        {
                                                lastCoverageLine = null;

                                                html.Append("</span>");
                                        }
                                }

                                while (currentCoverageLine != null && currentCoverageLine.HitTest(token.Line, token.Column) == Position.After)
                                {
                                        if (lines.MoveNext())
                                                currentCoverageLine = lines.Current;
                                        else
                                                currentCoverageLine = null;
                                }

                                if (currentCoverageLine != null && currentCoverageLine.HitTest(token.Line, token.Column) == Position.Within)
                                {
                                        lastCoverageLine = lines.Current;
                                        if (lines.MoveNext())
                                                currentCoverageLine = lines.Current;
                                        else
                                                currentCoverageLine = null;

                                        if ((lastCoverageLine.Coverage & 1) != 0)
                                                html.Append($"<span class='Touched'>");
                                        else if ((lastCoverageLine.Coverage & 2) != 0)
                                                html.Append($"<span class='PartiallyTouched'>");
                                        else
                                                html.Append($"<span class='NotTouched'>");
                                }

                                if (lastLine == null || lastLine.Value != token.Line)
                                {
                                        lastLine = token.Line;

                                        html.AppendFormat("{0,5} ", token.Line);
                                }

                                switch (token.TokenCategory)
                                {
                                        case TokenCategory.Undefined:
                                        case TokenCategory.Whitespace:
                                        case TokenCategory.Newline:
                                                html.Append(token.Text);
                                                break;

                                        case TokenCategory.Directive:
                                                html.Append($"<span class='{token.TokenCategory.ToString()}'>{System.Security.SecurityElement.Escape(token.Text)}</span>");
                                                break;

                                        default:
                                                html.Append($"<span class='{token.TokenCategory.ToString()}'>{token.Text}</span>");
                                                break;
                                }
                        }

                        html.Append(@"
</code>
</pre>
</body>
</html>");
                        File.WriteAllText(outputPath, html.ToString());
                }
        }
}
