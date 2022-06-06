using Coverage2Html.Core.CoverageData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core
{
        internal class CoverageDataCache
        {
                public IList<SourceFile> SourceFiles { get; set; }
        }
        public class CoverageJsonFile
	{
                public static void WriteJson(IList<SourceFile> sourceFiles, string filename)
                {
                        var cache = new CoverageDataCache() { SourceFiles = sourceFiles };

                        string json = JsonConvert.SerializeObject(cache, Formatting.Indented);

                        File.WriteAllText(filename, json);
                }

                public static IList<SourceFile> ReadJson(string filename)
                {
                        var json = File.ReadAllText(filename);

                        var cache = JsonConvert.DeserializeObject<CoverageDataCache>(json);

                        return cache.SourceFiles;
                }

        }
}
