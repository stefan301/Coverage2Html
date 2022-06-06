using Antlr4.Runtime;
using CppParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core.Tokens
{
	public class JsonTokenMapping
	{
		public string Type { get; set; }

		[JsonConverter(typeof(StringEnumConverter))] 
		public TokenCategory Category { get; set; }
	}

	public class TokenMappingJsonFile
	{
		public static void InitTokenMapping(string filename, Vocabulary vocabulary)
		{
			var mapping = new List<JsonTokenMapping>();

			foreach (var item in GetTokenTypes(vocabulary))
			{
				mapping.Add(new JsonTokenMapping() { Type = item.Key, Category = TokenCategory.Undefined });

			}

			string json = JsonConvert.SerializeObject(mapping, Formatting.Indented);

			File.WriteAllText(filename, json);
		}

		public static Dictionary<int, TokenCategory> ReadTokenMapping( string filename, Vocabulary vocabulary, TextWriter errorOutput)
		{
			var mapping = new Dictionary<int, TokenCategory>();

			try
			{
				var json = File.ReadAllText(filename);

				var jsonMapping = JsonConvert.DeserializeObject<List<JsonTokenMapping>>(json);

				var types = GetTokenTypes(vocabulary);

				foreach (JsonTokenMapping jm in jsonMapping)
				{
					if (types.TryGetValue(jm.Type, out int tokenType))
					{
						try
						{
							mapping.Add(tokenType, jm.Category);
						}
						catch (Exception ex)
						{
							errorOutput.WriteLine($"ReadTokenMapping({filename}): failed to add mapping {tokenType}/{jm.Category}: {ex.Message}");
						}
					}
				}
			}
			catch(Exception ex)
			{
				errorOutput.WriteLine($"failed to read token mapping from {filename}: {ex.Message}");
			}

			return mapping;
		}

		private static Dictionary<string, int> GetTokenTypes(Vocabulary vocabulary)
		{
			var types = new Dictionary<string, int>();

			for (int tokenType = 1; tokenType <= vocabulary.getMaxTokenType(); ++tokenType)
			{
				types.Add(vocabulary.GetSymbolicName(tokenType), tokenType);
			}

			return types;
		}
	}
}
