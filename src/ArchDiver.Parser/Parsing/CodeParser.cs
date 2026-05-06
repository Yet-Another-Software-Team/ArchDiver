using TreeSitterParser = TreeSitter.Parser;
using TreeSitter;
using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;
using ArchDiver.Parser.Infrastructure;

namespace ArchDiver.Parser.Parsing;

/// <summary>
/// A parser binding using Tree-sitter.
/// </summary>
public class CodeParser(ILanguageProvider provider)
{
    private readonly Language _language = InitializeLanguage(provider);

    private static Language InitializeLanguage(ILanguageProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));

        try
        {
            string libraryName = PlatformHelper.GetPlatformLibraryName(provider.BaseLibraryName);
            return new Language(libraryName, provider.FunctionName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load Tree-sitter native library for {provider.LanguageId}.", ex);
        }
    }

    /// <summary>
    /// Parses the provided source code into an Abstract Syntax Tree.
    /// </summary>
    /// <param name="sourceCode">The code to parse.</param>
    /// <returns>The root node of the generated AST.</returns>
    public AstNode Parse(string sourceCode)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            throw new ArgumentException("Source code cannot be null or empty.", nameof(sourceCode));
        }

        using var parser = new TreeSitterParser(_language);
        using var tree = parser.Parse(sourceCode);

        if (tree == null || tree.RootNode == null)
        {
            throw new Exception("Tree-sitter failed to parse the source code.");
        }

        return MapToAstNode(tree.RootNode, sourceCode);
    }

    private AstNode MapToAstNode(Node tsNode, string sourceCode)
    {
        var node = new AstNode
        {
            Type = tsNode.Type,
            Text = tsNode.Text ?? string.Empty,
            Range = new SourceRange(
                new SourcePoint(tsNode.StartPosition.Row, tsNode.StartPosition.Column, tsNode.StartIndex),
                new SourcePoint(tsNode.EndPosition.Row, tsNode.EndPosition.Column, tsNode.EndIndex)
            )
        };

        for (int i = 0; i < tsNode.Children.Count(); i++)
        {
            var tsChild = tsNode.Children.ElementAt(i);
            if (tsChild != null)
            {
                var childNode = MapToAstNode(tsChild, sourceCode);
                childNode.FieldName = tsNode.GetFieldNameForChild(i);
                node.AddChild(childNode);
            }
        }
        return node;
    }
}
