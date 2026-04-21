using System;
using System.IO;
using System.Linq;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Infrastructure;

namespace ArchDiver.Core.Languages;

[NodeBinding("Method", "method_declaration", "constructor_declaration")]
[NodeBinding("Class", "class_declaration", "interface_declaration", "enum_declaration", "record_declaration")]
[NodeBinding("Field", "field_declaration")]
[NodeBinding("Import", "import_declaration")]
[NodeBinding("Identifier", "identifier")]
public class JavaLanguageProvider : LanguageProviderBase
{
    public override string LanguageId => "Java";
    public override string BaseLibraryName => "tree-sitter-java";
    public override string FunctionName => "tree_sitter_java";

    private static readonly string[] _extensions = { ".java" };

    public override bool CanHandle(string filePath, string content)
    {
        string ext = Path.GetExtension(filePath);
        return _extensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
    }
}
