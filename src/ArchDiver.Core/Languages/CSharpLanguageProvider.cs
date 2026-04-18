using System;
using System.IO;
using System.Linq;
using ArchDiver.Core.Abstractions;

namespace ArchDiver.Core.Languages
{
    public class CSharpLanguageProvider : ILanguageProvider
    {
        public string LanguageId => "CSharp";
        public string LibraryName => "tree-sitter-c-sharp.dll";
        public string FunctionName => "tree_sitter_c_sharp";

        private static readonly string[] _extensions = { ".cs" };

        public bool CanHandle(string filePath, string content)
        {
            string ext = Path.GetExtension(filePath);
            return _extensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
    }
}
