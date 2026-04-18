using System;
using System.Collections.Generic;
using System.Linq;
using TreeSitter;

namespace ArchDiver.Core
{
    /// <summary>
    /// Represents a specific point in the source code.
    /// </summary>
    public record struct SourcePoint(int Line, int Column, int Offset);

    /// <summary>
    /// Represents a range within the source code.
    /// </summary>
    public record struct SourceRange(SourcePoint Start, SourcePoint End);

    /// <summary>
    /// Represents a generic node in the Abstract Syntax Tree (AST).
    /// </summary>
    public class AstNode
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public SourceRange Range { get; set; }
        public AstNode? Parent { get; set; }
        public List<AstNode> Children { get; set; } = new List<AstNode>();

        /// <summary>
        /// Adds a child node and sets its parent reference.
        /// </summary>
        public void AddChild(AstNode child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            child.Parent = this;
            Children.Add(child);
        }
    }

    /// <summary>
    /// A parser binding using Tree-sitter.
    /// </summary>
    public class CodeParser
    {
        public Language Language => _language;
        private readonly Language _language;

        public CodeParser(string languageName = "CSharp")
        {
            try
            {
                if (languageName == "CSharp")
                {
                    // Explicitly use the library name and function name that actually exist
                    _language = new Language("tree-sitter-c-sharp.dll", "tree_sitter_c_sharp");
                }
                else
                {
                    _language = new Language(languageName);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize Tree-sitter language '{languageName}'.", ex);
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

            using var parser = new Parser(_language);
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

            foreach (var tsChild in tsNode.Children)
            {
                if (tsChild != null)
                {
                    node.AddChild(MapToAstNode(tsChild, sourceCode));
                }
            }

            return node;
        }
    }
}
