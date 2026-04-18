using System;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Models;
using ArchDiver.Core.Parsing;
using ArchDiver.Core.Storage;

namespace ArchDiver.Core.Pipeline
{
    /// <summary>
    /// Acts as the central controller and brain of the software, orchestrating the data pipeline.
    /// </summary>
    public class PipelineControlUnit
    {
        private readonly ContextStorage _contextStorage;

        public PipelineControlUnit(ContextStorage? contextStorage = null)
        {
            _contextStorage = contextStorage ?? new ContextStorage();
        }

        /// <summary>
        /// Processes the given source code through the pipeline.
        /// Currently only parses the code into an Abstract Syntax Tree (AST).
        /// </summary>
        /// <param name="sourceCode">The source code to process.</param>
        /// <param name="languageId">The language ID to use for parsing.</param>
        /// <returns>The root node of the generated AST.</returns>
        public AstNode Process(string sourceCode, string languageId = "CSharp")
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));
            }

            Console.WriteLine($"PipelineControlUnit: Initiating parsing phase for {languageId}...");

            var provider = LanguageRegistry.GetById(languageId)
                           ?? throw new NotSupportedException($"Language '{languageId}' is not registered.");

            // Step 1: Parse the code into an AST
            var parser = new CodeParser(provider);
            AstNode ast = parser.Parse(sourceCode);

            Console.WriteLine($"PipelineControlUnit: Parsing complete. Generated AST root node of type '{ast.Type}'.");

            // Store the parsed AST in the context storage
            _contextStorage.StoreAst(ast);

            // TODO: Future pipeline stages (e.g., semantic analysis, indexing, exporting) will go here.

            return ast;
        }
    }
}
