using ArchDiver.Core;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Models;
using ArchDiver.Core.Parsing;
using ArchDiver.Core.Languages;
using Xunit;

namespace ArchDiver.Tests;

public class CodeParserTests
{
    public CodeParserTests()
    {
        Bootstrapper.Initialize();
    }

    [Fact]
    public void Parse_ValidCSharp_ReturnsAstWithExpectedRoot()
    {
        // Arrange
        var provider = LanguageRegistry.GetById("CSharp")!;
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
        var provider = LanguageRegistry.GetById("CSharp")!;
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
        var provider = LanguageRegistry.GetById("Python")!;
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
    public void Parse_ValidJava_ReturnsAst()
    {
        // Arrange
        var provider = LanguageRegistry.GetById("Java")!;
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
