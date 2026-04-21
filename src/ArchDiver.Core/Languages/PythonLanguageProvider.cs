using System;
using System.IO;
using System.Linq;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Infrastructure;

namespace ArchDiver.Core.Languages
{
    [NodeBinding("Method", "function_definition")]
    [NodeBinding("Class", "class_definition")]
    [NodeBinding("Field", "assignment")]
    [NodeBinding("Import", "import_statement", "import_from_statement")]
    [NodeBinding("Identifier", "identifier")]
    public class PythonLanguageProvider : LanguageProviderBase
    {
        public override string LanguageId => "Python";
        public override string BaseLibraryName => "tree-sitter-python";
        public override string FunctionName => "tree_sitter_python";

        private static readonly string[] _extensions = { ".py" };

        public override bool CanHandle(string filePath, string content)
        {
            string ext = Path.GetExtension(filePath);
            return _extensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
    }
}
