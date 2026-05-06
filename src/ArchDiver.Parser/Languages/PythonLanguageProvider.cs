using ArchDiver.Parser.Abstractions;
using ArchDiver.Parser.Infrastructure;

namespace ArchDiver.Parser.Languages;

[NodeBinding("Method", "function_definition")]
[NodeBinding("MethodName", "identifier")]
[NodeBinding("Class", "class_definition")]
[NodeBinding("ClassName", "identifier")]
[NodeBinding("Field", "assignment", "expression_statement")]
[NodeBinding("FieldName", "identifier")]
[NodeBinding("Import", "import_statement", "import_from_statement")]
[NodeBinding("ImportName", "dotted_name", "identifier")]
[NodeBinding("Identifier", "identifier", "dotted_name")]
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
