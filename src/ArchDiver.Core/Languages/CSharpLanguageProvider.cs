using System;
using System.IO;
using System.Linq;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Infrastructure;

namespace ArchDiver.Core.Languages;

[NodeBinding("Method", "method_declaration", "constructor_declaration", "destructor_declaration", "operator_declaration", "conversion_operator_declaration")]
[NodeBinding("Class", "class_declaration", "struct_declaration", "record_declaration", "interface_declaration", "enum_declaration")]
[NodeBinding("Field", "field_declaration", "property_declaration", "event_declaration")]
[NodeBinding("Import", "using_directive")]
[NodeBinding("Identifier", "identifier")]
public class CSharpLanguageProvider : LanguageProviderBase
{
    public override string LanguageId => "CSharp";
    public override string BaseLibraryName => "tree-sitter-c-sharp";
    public override string FunctionName => "tree_sitter_c_sharp";

    private static readonly string[] _extensions = { ".cs" };

    public override bool CanHandle(string filePath, string content)
    {
        string ext = Path.GetExtension(filePath);
        return _extensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
    }
}
