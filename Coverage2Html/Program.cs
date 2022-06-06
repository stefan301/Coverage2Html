using Antlr4.Runtime;
using CommandLine;
using CommandLine.Text;
using Coverage2Html.Core;
using Coverage2Html.Core.CoverageData;
using Coverage2Html.Core.Html;
using Coverage2Html.Core.Tokens;
using CppParser;
using CsParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html
{
        internal class Program
        {
                static int Main(string[] args)
                {
                        var instance = new Program();

                        var result = CommandLine.Parser.Default.ParseArguments<Options>(args);
                        var resultRun = result.MapResult(instance.Run, _ => {
                                return false;
                        }) ? 0 : 1;

                        if (resultRun == 1)
                        {
                                var helpText = HelpText.AutoBuild(result, h => h, e =>
                                {
                                        return e;
                                });
                                Console.WriteLine(helpText);
                        }

                        return resultRun;
                }

                private TextWriter Output { get; set; } = Console.Out;

                private TextWriter ErrorOutput { get; set; } = Console.Error;

                private readonly Dictionary<string, ISourceFileReader> _sourceFileReaders = new Dictionary<string, ISourceFileReader>();

                private string ExeFilePath 
                { 
                        get 
                        {
                                return System.Reflection.Assembly.GetExecutingAssembly().Location; 
                        } 
                }
                private string ExeDirectory 
                { 
                        get 
                        { 
                                return Path.GetDirectoryName(ExeFilePath); 
                        } 
                }
                private string HtmlDirectory
                {
                        get
                        {
                                return Path.Combine(ExeDirectory, "html");
                        }
                }
                private string TokenDirectory
                {
                        get
                        {
                                return Path.Combine(ExeDirectory, "tokens");
                        }
                }

                private bool Run(Options options)
                {
                        foreach (var item in options.CoverageFiles)
			{
                                if( !File.Exists(item))
				{
                                        ErrorOutput.WriteLine($"coverage file {item} does not exist.");
                                        return false;
				}
			}

                        Output.WriteLine($"{DateTime.Now}: reading coverage files: {string.Join(", ", options.CoverageFiles)}");

                        IList<SourceFile> sourceFiles = null;

                        try
                        {
                                if (options.CoverageFiles.Count() == 1 && Path.GetExtension(options.CoverageFiles.First()) == ".json")
                                {
                                        sourceFiles = CoverageJsonFile.ReadJson(options.CoverageFiles.First());
                                }
                                else
                                {
                                        var coverageParser = new CoverageParser(options.RootFolder, options.ExcludePaths, Output, ErrorOutput);
                                        coverageParser.ShowExcluded = options.ShowExcluded;

                                        sourceFiles = coverageParser.Parse(options.CoverageFiles.ToArray());
                                }
                        }
                        catch (Exception ex)
			{
                                ErrorOutput.WriteLine($"{DateTime.Now}: failed reading coverage data: {ex.ToString()}");
                                return false;
			}

                        if (sourceFiles != null)
                        {
                                Output.WriteLine($"{DateTime.Now}: done reading coverage files");
                        }
                        else
			{
                                ErrorOutput.WriteLine($"{DateTime.Now}: failed reading coverage data");
                                return false;
                        }

                        try
                        {
                                if (!string.IsNullOrEmpty(options.JsonCoverageFile))
                                {
                                        CoverageJsonFile.WriteJson(sourceFiles, options.JsonCoverageFile);
                                }
                        }
                        catch (Exception ex)
                        {
                                ErrorOutput.WriteLine($"{DateTime.Now}: failed writting coverage data to {options.JsonCoverageFile}: {ex.ToString()}");
                                return false;
                        }

                        var outputFolder = options.OutputFolder;
                        var rootFolder = options.RootFolder;

                        if (!Directory.Exists(outputFolder))
                                Directory.CreateDirectory(outputFolder);

                        CopyFile("styles.css", HtmlDirectory, outputFolder);

                        string cssPath = Path.Combine(outputFolder, "styles.css");

                        RegisterSourceFileReaders();

                        foreach (var sourceFile in sourceFiles)
                        {
                                if (File.Exists(sourceFile.Path) && sourceFile.Lines.Any())
                                {
                                        string absolutePath = Path.GetFullPath(sourceFile.Path);
                                        string relativePath = Utils.MakeRelativePath(rootFolder, absolutePath);
                                        string outputPath = Path.Combine(outputFolder, relativePath + ".html");
                                        string outputDir = Path.GetDirectoryName(outputPath);
                                        string relativeCssPath = Utils.MakeRelativePath(outputPath, cssPath);

                                        if( !Directory.Exists(outputDir) )
                                                Directory.CreateDirectory(outputDir);

                                        var ext = Path.GetExtension(absolutePath);
                                        if( _sourceFileReaders.TryGetValue(ext, out var reader) )
					{
                                                var tokenStream = reader.ReadSourceFile(absolutePath, Output, ErrorOutput);

                                                HtmlGenerator.WriteHtml(tokenStream, sourceFile, outputPath, relativeCssPath);
					}
                                }
                        }

                        return true;
                }

                private void RegisterSourceFileReaders()
                {
                        RegisterSourceFileReader<CPP14Lexer>(new string[] { ".h", ".c", ".cpp" },
                                (s, o, e) => new CPP14Lexer(s, o, e),
                                TokenMappingJsonFile.ReadTokenMapping(Path.Combine(TokenDirectory, "cpp.json"), (Vocabulary)CPP14Lexer.DefaultVocabulary, ErrorOutput));

                        RegisterSourceFileReader<CSharpLexer>(new string[] { ".cs" },
                                (s, o, e) => new CSharpLexer(s, o, e),
                                TokenMappingJsonFile.ReadTokenMapping(Path.Combine(TokenDirectory, "cs.json"), (Vocabulary)CSharpLexer.DefaultVocabulary, ErrorOutput));
                }
                private void RegisterSourceFileReader<T>(string[] extensions, Func<ICharStream, TextWriter, TextWriter, T> lexerConstructor, Dictionary<int, TokenCategory> tokenCategories) where T : Lexer
                {
                        var reader = new SourceFileReader<T>(lexerConstructor, tokenCategories);

                        foreach (var extension in extensions)
                                _sourceFileReaders.Add(extension, reader);
                }

                void CopyFile(string fileName, string inputFolder, string outputFolder)
                {
                        string sourceFilePath = Path.Combine(inputFolder, fileName);
                        string destinationFilePath = Path.Combine(outputFolder, fileName);

                        try
                        {
                                File.Copy(sourceFilePath, destinationFilePath, true);
                        }
                        catch (Exception ex)
                        {
                                Console.WriteLine($"Failed to copy {sourceFilePath} to {destinationFilePath}: {ex.Message}");
                        }
                }

        }
}
