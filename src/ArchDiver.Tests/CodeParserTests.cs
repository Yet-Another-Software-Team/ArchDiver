using ArchDiver.Core;
using Xunit;

namespace ArchDiver.Tests
{
    public class CodeParserTests
    {
        [Fact]
        public void Parse_ValidCSharp_ReturnsAstWithExpectedRoot()
        {
            // Arrange
            var parser = new CodeParser("CSharp");
            var sourceCode = "class Program { void Main() {} }";

            // Act
            var ast = parser.Parse(sourceCode);

            // Assert
            Assert.NotNull(ast);
            Assert.Equal("compilation_unit", ast.Type); // Tree-sitter's root for C# is usually compilation_unit
            Assert.NotEmpty(ast.Children);
        }

        [Fact]
        public void Parse_SmallSnippet_ContainsCorrectMetadata()
        {
            // Arrange
            var parser = new CodeParser("CSharp");
            var sourceCode = "// Hello";

            // Act
            var ast = parser.Parse(sourceCode);

            // Assert
            Assert.NotNull(ast);
            Assert.Contains(ast.Children, c => c.Type == "comment" && c.Text == "// Hello");
            Assert.Equal(0, ast.Range.Start.Line);
            Assert.Equal(8, ast.Range.End.Column);
        }
    }
}
