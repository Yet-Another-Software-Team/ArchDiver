using System;
using System.IO;
using System.Linq;
using ArchDiver.Core.Abstractions;

namespace ArchDiver.Core.Languages
{
    public class JavaLanguageProvider : ILanguageProvider
    {
        public string LanguageId => "Java";
        public string BaseLibraryName => "tree-sitter-java";
        public string FunctionName => "tree_sitter_java";

        private static readonly string[] _extensions = { ".java" };

        public bool CanHandle(string filePath, string content)
        {
            string ext = Path.GetExtension(filePath);
            return _extensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
    }
}
