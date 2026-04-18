using System;
using System.Collections.Generic;

namespace ArchDiver.Core.Models
{
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
}
