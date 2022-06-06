using Antlr4.Runtime;
using Coverage2Html.Core.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core
{
	public class SourceFileReader<T> : ISourceFileReader where T : Lexer
	{
                private readonly Func<ICharStream, TextWriter, TextWriter, T> _lexerConstructor;

                private readonly Dictionary<int, TokenCategory> _tokenCategories;
                public SourceFileReader(Func<ICharStream, TextWriter, TextWriter, T> lexerConstructor, Dictionary<int, TokenCategory> tokenCategories)
		{
                        _lexerConstructor = lexerConstructor;
                        _tokenCategories = tokenCategories;
                }
                public CommonTokenStream ReadSourceFile(string path, TextWriter output, TextWriter errorOutput)
                {
                        output.WriteLine($"{DateTime.Now}: ReadSourceFile({path})");

                        var encoding = Utils.GetEncoding(path);
                        var stream = CharStreams.fromPath(path, encoding);
                        var tokenFacory = new HtmlCommonTokenFactory();

                        var lexer = _lexerConstructor(stream, output, errorOutput);
                        lexer.TokenFactory = tokenFacory;

                        var tokens = new CommonTokenStream(lexer);
                        tokens.Fill();

                        foreach (HtmlCommonToken token in tokens.GetTokens())
                        {
                                if (token.Type == Lexer.Eof)
                                        continue;

                                token.TokenName = lexer.Vocabulary.GetSymbolicName(token.Type);

                                if (_tokenCategories.TryGetValue(token.Type, out var category))
                                        token.TokenCategory = category;
                                else
                                        errorOutput.WriteLine($"Tokentype {token.Type} not found in dictionary ({token.TokenName})");
                        }

                        return tokens;
                }
        }
}
