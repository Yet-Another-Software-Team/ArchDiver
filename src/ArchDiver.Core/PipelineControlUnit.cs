using System;

namespace ArchDiver.Core
{
    /// <summary>
    /// Acts as the central controller and brain of the software, orchestrating the data pipeline.
    /// </summary>
    public class PipelineControlUnit
    {
        private readonly CodeParser _parser;
        private readonly ContextStorage _contextStorage;

        public PipelineControlUnit(ContextStorage? contextStorage = null)
        {
            // Initialize the parser that will be used in the pipeline
            _parser = new CodeParser();
            _contextStorage = contextStorage ?? new ContextStorage();
        }

        /// <summary>
        /// Processes the given source code through the pipeline.
        /// Currently only parses the code into an Abstract Syntax Tree (AST).
        /// </summary>
        /// <param name="sourceCode">The source code to process.</param>
        /// <returns>The root node of the generated AST.</returns>
        public AstNode Process(string sourceCode)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));
            }

            Console.WriteLine("PipelineControlUnit: Initiating parsing phase...");

            // Step 1: Parse the code into an AST
            AstNode ast = _parser.Parse(sourceCode);

            Console.WriteLine($"PipelineControlUnit: Parsing complete. Generated AST root node of type '{ast.Type}'.");

            // Store the parsed AST in the context storage
            _contextStorage.StoreAst(ast);

            // TODO: Future pipeline stages (e.g., semantic analysis, indexing, exporting) will go here.

            return ast;
        }
    }
}
