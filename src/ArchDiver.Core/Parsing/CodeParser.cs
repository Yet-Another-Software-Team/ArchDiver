using System;
using System.Collections.Generic;
using System.Linq;
using TreeSitter;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Parsing
{
    /// <summary>
    /// A parser binding using Tree-sitter.
    /// </summary>
    public class CodeParser
    {
        public Language Language => _language;
        private readonly Language _language;

        public CodeParser(ILanguageProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            try
            {
                string libraryName = Infrastructure.PlatformHelper.GetPlatformLibraryName(provider.BaseLibraryName);
                _language = new Language(libraryName, provider.FunctionName);
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
