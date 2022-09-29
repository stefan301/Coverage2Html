using Coverage2Html.Core.CoverageData;
using Microsoft.CodeCoverage.Analysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core
{
        public class CoverageParser
        {
                protected readonly TextWriter Output;

                protected readonly TextWriter ErrorOutput;

                protected readonly string RootFolder;

                protected readonly IEnumerable<string> ExcludePaths;

                public bool ShowExcluded { get; set; } = false;

		public CoverageParser(string rootFolder, IEnumerable<string> excludePaths)
                : this(rootFolder, excludePaths, Console.Out, Console.Error) { }
                public CoverageParser(string rootFolder, IEnumerable<string> excludePaths, TextWriter output, TextWriter errorOutput)
                {
                        this.RootFolder = rootFolder;
                        this.ExcludePaths = excludePaths;
                        this.Output = output;
                        this.ErrorOutput = errorOutput;
                }
                private static CoverageInfo JoinCoverageFiles(String[] files)
                {
                        CoverageInfo result = null;

                        try
                        {
                                foreach (String file in files)
                                {
                                        CoverageInfo current = CoverageInfo.CreateFromFile(file);

                                        if (result == null)
                                        {
                                                result = current;
                                                continue;
                                        }

                                        CoverageInfo joined = null;

                                        try
                                        {
                                                joined = CoverageInfo.Join(result, current);
                                        }
                                        finally
                                        {
                                                current.Dispose();
                                                result.Dispose();
                                        }

                                        result = joined;
                                }
                        }
                        catch (Exception)
                        {
                                if (result != null)
                                {
                                        result.Dispose();
                                }
                                throw;
                        }

                        return result;
                }

                public IList<SourceFile> Parse(String[] inputfiles)
                {
                        using (CoverageInfo info = JoinCoverageFiles(inputfiles))
                        {
                                CoverageDS dataSet = info.BuildDataSet();

                                // Namespaces
                                DataTable namespacesTable = dataSet.Tables["NameSpaceTable"];
                                DataTable classesTable = dataSet.Tables["Class"];
                                DataTable filesTable = dataSet.Tables["SourceFileNames"];

                                Dictionary<uint, SourceFile> files = CreateFileIndex(filesTable);

                                foreach (DataRow iclass in classesTable.Rows)
                                {
                                        DataRow[] methodRows = iclass.GetChildRows("Class_Method");

                                        uint fileid = 0;

                                        foreach (DataRow imethod in methodRows)
                                        {
                                                // Method starting line
                                                uint lnStart = 0;
                                                // Get First Line in class
                                                DataRow[] lineRows = imethod.GetChildRows("Method_Lines");

                                                foreach (DataRow iline in lineRows)
                                                {
                                                        SourceLine le = null;
                                                        uint coverage = iline.Field<uint>("Coverage");
                                                        if (lnStart == 0)
                                                        {
                                                                fileid = iline.Field<uint>("SourceFileID");
                                                                string methodname = WebUtility.HtmlEncode((string)imethod["MethodName"]);
                                                                lnStart = iline.Field<uint>("LnStart");
                                                                uint colStart = iline.Field<uint>("ColStart");
                                                                uint lnEnd = iline.Field<uint>("LnEnd");
                                                                uint colEnd = iline.Field<uint>("ColEnd");

                                                                le = new SourceLine(lnStart, colStart, lnEnd, colEnd, "method", methodname );
                                                                le.SetCoverageBit((int)coverage);
                                                        }
                                                        else
                                                        {
                                                                lnStart = iline.Field<uint>("LnStart");
                                                                uint colStart = iline.Field<uint>("ColStart");
                                                                uint lnEnd = iline.Field<uint>("LnEnd");
                                                                uint colEnd = iline.Field<uint>("ColEnd");

                                                                le = new SourceLine(lnStart, colStart, lnEnd, colEnd, "stmt", string.Empty );
                                                                le.SetCoverageBit((int)coverage);
                                                        }

                                                        if (files.ContainsKey(fileid))
                                                        {
                                                                SourceFile fe = files[fileid];

                                                                fe.AddLine(le);
                                                        }
                                                }
                                        }
                                }

                                return files.Values.ToArray();
                        }
                }
                private Dictionary<uint, SourceFile> CreateFileIndex(DataTable filesTable)
                {
                        var index = new Dictionary<uint, SourceFile>();

                        foreach (DataRow row in filesTable.Rows)
                        {
                                string fname = (string)row["SourceFileName"];
                                try
                                {
                                        FileInfo info = new FileInfo(fname);
                                        if (Included(info) && !Excluded(info))
                                        {
                                                uint fileid = (uint)row["SourceFileID"];
                                                string fullName = info.FullName;

                                                var file = new SourceFile(fullName);

                                                index.Add(fileid, file);
                                        }
                                }
                                catch (Exception e)
                                {
                                        ErrorOutput.WriteLine($"Error reading file {fname}. Message = {e.Message}");
                                }
                        }

                        return index;
                }

                private bool Included(FileInfo info)
		{
                        if( FileIsSubpathOf(RootFolder, info))
			{
                                return true;
			}
			else
			{
                                if( ShowExcluded )
                                        Output.WriteLine($"{info.FullName} not selected");
                                return false;
                        }
                }

                private bool Excluded(FileInfo info)
		{
                        foreach (var exclude in ExcludePaths)
                        {
                                if (Directory.Exists(exclude) && FileIsSubpathOf(exclude, info))
                                {
                                        if (ShowExcluded)
                                                Output.WriteLine($"{info.FullName} excluded by {exclude}");
                                        return true;
                                }
                                else if( File.Exists(exclude) )
				{
                                        var fi = new FileInfo(exclude);
                                        if( fi.FullName == info.FullName )
					{
                                                if (ShowExcluded)
                                                        Output.WriteLine($"{info.FullName} excluded by {exclude}");
                                                return true;
                                        }
                                }
                        }
                        return false;
                }
                private bool FileIsSubpathOf(string directory, FileInfo file)
                {
                        for (DirectoryInfo fileDir = file.Directory, dir = new DirectoryInfo(directory); fileDir != null; fileDir = fileDir.Parent)
                        {
				if (string.Compare(fileDir.FullName, dir.FullName, true) == 0)
					return true;
			}

			return false;
                }
        }
}
