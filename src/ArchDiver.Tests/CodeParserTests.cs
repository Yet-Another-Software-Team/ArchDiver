using ArchDiver.Core.Models;
using ArchDiver.Parser.Parsing;
using ArchDiver.Parser.Languages;
using ArchDiver.Parser.Abstractions;
using ArchDiver.Parser.Infrastructure;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArchDiver.Tests;

public class CodeParserTests
{
    private readonly ILanguageRegistry _registry;

    public CodeParserTests()
    {
        _registry = new LanguageRegistry();
        _registry.Register(new CSharpLanguageProvider());
        _registry.Register(new PythonLanguageProvider());
        _registry.Register(new JavaLanguageProvider());
    }

    [Fact]
    public void NodeBindings_MultipleAttributes_AreMerged()
    {
        // Arrange & Act
        var provider = _registry.GetById("CSharp")!;
        var bindings = provider.NodeBindings;

        // Assert
        Assert.Contains("compilation_unit", bindings["Root"]);
        Assert.Contains("translation_unit", bindings["Root"]);
        Assert.Contains("method_declaration", bindings["Method"]);
        Assert.Contains("class_declaration", bindings["Class"]);
    }

    [Fact]
    public void Parse_ValidCSharp_ReturnsAstWithExpectedRoot()
    {
        // Arrange
        var provider = _registry.GetById("CSharp")!;
        var parser = new CodeParser(provider);
        var sourceCode = "class Program { void Main() {} }";

        // Act
        var ast = parser.Parse(sourceCode);

        // Assert
        Assert.NotNull(ast);
        Assert.Equal("compilation_unit", ast.Type); 
        Assert.NotEmpty(ast.Children);
    }

    [Fact]
    public void Parse_SmallSnippet_ContainsCorrectMetadata()
    {
        // Arrange
        var provider = _registry.GetById("CSharp")!;
        var parser = new CodeParser(provider);
        var sourceCode = "// Hello";

        // Act
        var ast = parser.Parse(sourceCode);

        // Assert
        Assert.NotNull(ast);
        Assert.Contains(ast.Children, c => c.Type == "comment" && c.Text == "// Hello");
        Assert.Equal(0, ast.Range.Start.Line);
        Assert.Equal(8, ast.Range.End.Column);
    }

    [Fact]
    public void Parse_ValidPython_ReturnsAst()
    {
        // Arrange
        var provider = _registry.GetById("Python")!;
        var parser = new CodeParser(provider);
        var sourceCode = "def main():\n    print('Hello')";

        // Act
        var ast = parser.Parse(sourceCode);

        // Assert
        Assert.NotNull(ast);
        Assert.Equal("module", ast.Type); // Python root node is usually module
        Assert.NotEmpty(ast.Children);
    }

    [Fact]
    public void Parse_CSharpClass_HasFieldNameForName()
    {
        // Arrange
        var provider = _registry.GetById("CSharp")!;
        var parser = new CodeParser(provider);
        var sourceCode = "class MyClass {}";

        // Act
        var ast = parser.Parse(sourceCode);

        // Assert
        var classNode = FindNode(ast, "class_declaration");
        Assert.NotNull(classNode);
        var nameNode = classNode.Children.FirstOrDefault(c => c.FieldName == "name");
        Assert.NotNull(nameNode);
        Assert.Equal("MyClass", nameNode.Text);
    }

    private AstNode? FindNode(AstNode node, string type)
    {
        if (node.Type == type) return node;
        foreach (var child in node.Children)
        {
            var found = FindNode(child, type);
            if (found != null) return found;
        }
        return null;
    }

    [Fact]
    public void Parse_ValidJava_ReturnsAst()
    {
        // Arrange
        var provider = _registry.GetById("Java")!;
        var parser = new CodeParser(provider);
        var sourceCode = "class Main { public static void main(String[] args) {} }";

        // Act
        var ast = parser.Parse(sourceCode);

        // Assert
        Assert.NotNull(ast);
        Assert.Equal("program", ast.Type); // Java root node is usually program
        Assert.NotEmpty(ast.Children);
    }
}
