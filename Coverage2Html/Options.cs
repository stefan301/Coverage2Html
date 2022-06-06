using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html
{
	internal class Options
	{
		[Option('f', "files", Required = true, HelpText = ".coverage files for .html file generation")]
		public IEnumerable<string> CoverageFiles { get; set; }

		[Option('o', "output", Required = true, HelpText = "Output folder for the .html files")]
		public string OutputFolder { get; set; }

		[Option('r', "root", Required = true, HelpText = "Root folder of the source files")]
		public string RootFolder { get; set; }

		[Option('e', "exclude", Required = false, HelpText = "source file paths to exclude")]
		public IEnumerable<string> ExcludePaths { get; set; }

		[Option("show-excluded", Required = false, HelpText = "show excluded source files")]
		public bool ShowExcluded { get; set; }

		[Option("cov-json", Required = false, HelpText = "write coverage data read from .coverage files to a json file")]
		public string JsonCoverageFile { get; set; }

		public static string GetUsage<T>(ParserResult<T> result)
		{
			return HelpText.AutoBuild(result, Parser.Default.Settings.MaximumDisplayWidth);
		}
	}
}
