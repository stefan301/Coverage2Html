using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coverage2Html.Core
{
	public static class Utils
	{
                /// <summary>
                /// Creates a relative path from one file or folder to another.
                /// </summary>
                /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
                /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
                /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
                /// <exception cref="ArgumentNullException"></exception>
                /// <exception cref="UriFormatException"></exception>
                /// <exception cref="InvalidOperationException"></exception>
                public static String MakeRelativePath(String fromPath, String toPath)
                {
                        if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
                        if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

                        Uri fromUri = new Uri(fromPath);
                        Uri toUri = new Uri(toPath);

                        if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

                        Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                        String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                        if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                        {
                                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                        }

                        return relativePath;
                }

                /// <summary>
                /// Determines a text file's encoding by analyzing its byte order mark (BOM).
                /// Defaults to ASCII when detection of the text file's endianness fails.
                /// </summary>
                /// <param name="filename">The text file to analyze.</param>
                /// <returns>The detected encoding.</returns>
                public static Encoding GetEncoding(string filename)
                {
                        // Read the BOM
                        var bom = new byte[4];
                        using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
                        {
                                file.Read(bom, 0, 4);
                        }

                        // Analyze the BOM
                        if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
                        if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
                        if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
                        if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
                        if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
                        if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

                        // We actually have no idea what the encoding is if we reach this point, so
                        // you may wish to return null instead of defaulting to ASCII
                        //            return Encoding.ASCII;
                        return Encoding.GetEncoding(1252);
                }
        }
}
