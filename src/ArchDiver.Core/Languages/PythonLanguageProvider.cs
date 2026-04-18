using System;
using System.IO;
using System.Linq;
using ArchDiver.Core.Abstractions;

namespace ArchDiver.Core.Languages
{
    public class PythonLanguageProvider : ILanguageProvider
    {
        public string LanguageId => "Python";
        public string BaseLibraryName => "tree-sitter-python";
        public string FunctionName => "tree_sitter_python";

        private static readonly string[] _extensions = { ".py" };

        public bool CanHandle(string filePath, string content)
        {
            string ext = Path.GetExtension(filePath);
            return _extensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
    }
}
