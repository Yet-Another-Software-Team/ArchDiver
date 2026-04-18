using System;
using System.IO;
using System.Linq;
using ArchDiver.Core;
using TreeSitter;

namespace ArchDiver.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }

            string command = args[0].ToLower();

            switch (command)
            {
                case "parse":
                    HandleParse(args);
                    break;
                case "query":
                    HandleQuery(args);
                    break;
                case "help":
                default:
                    PrintUsage();
                    break;
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("ArchDiver CLI");
            Console.WriteLine("Usage:");
            Console.WriteLine("  archdiver parse <file_path> [--lang <language>]");
            Console.WriteLine("  archdiver query <file_path> <query_string> [--lang <language>]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  parse    Parses a file and prints the AST structure.");
            Console.WriteLine("  query    Executes a Tree-sitter query against a file.");
        }

        static void HandleParse(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Missing file path.");
                return;
            }

            string filePath = args[1];
            string language = GetLanguageArg(args);
            bool raw = args.Contains("--raw");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                return;
            }

            try
            {
                string sourceCode = File.ReadAllText(filePath);
                var codeParser = new CodeParser(language);

                if (raw)
                {
                    using var parser = new Parser(codeParser.Language);
                    using var tree = parser.Parse(sourceCode);
                    Console.WriteLine($"Successfully parsed {filePath} ({language}) [RAW]");
                    PrintRawNode(tree.RootNode, 0);
                }
                else
                {
                    var ast = codeParser.Parse(sourceCode);
                    Console.WriteLine($"Successfully parsed {filePath} ({language})");
                    Console.WriteLine("AST Structure:");
                    PrintNode(ast, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing file: {ex.Message}");
            }
        }

        static void HandleQuery(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Error: Missing file path or query string.");
                return;
            }

            string filePath = args[1];
            string queryString = args[2];
            string languageName = GetLanguageArg(args);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                return;
            }

            try
            {
                string sourceCode = File.ReadAllText(filePath);
                var codeParser = new CodeParser(languageName);

                using var parser = new Parser(codeParser.Language);
                using var tree = parser.Parse(sourceCode);
                using var query = new Query(codeParser.Language, queryString);
                using var cursor = new QueryCursor();

                cursor.Execute(query, tree.RootNode);

                Console.WriteLine($"Query results for {filePath}:");
                int matchCount = 0;
                foreach (var match in cursor.Matches)
                {
                    matchCount++;
                    Console.WriteLine($"Match {matchCount}:");
                    foreach (var capture in match.Captures)
                    {
                        Console.WriteLine($"  - {capture.Name}: {capture.Node.Type} [{capture.Node.StartPosition.Row}:{capture.Node.StartPosition.Column}]");
                        Console.WriteLine($"    Text: \"{capture.Node.Text}\"");
                    }
                }

                if (matchCount == 0)
                {
                    Console.WriteLine("No matches found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
            }
        }

        static string GetLanguageArg(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--lang" && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            return "CSharp";
        }

        static void PrintNode(AstNode node, int indent)
        {
            string indentation = new string(' ', indent * 2);
            // We don't currently store field names in AstNode, but we can see them via the parse command if we had them.
            // Let's modify HandleParse to use a raw TreeSitter walk to show fields for debugging.
            Console.WriteLine($"{indentation}- {node.Type} [{node.Range.Start.Line}:{node.Range.Start.Column} - {node.Range.End.Line}:{node.Range.End.Column}]");

            if (!string.IsNullOrWhiteSpace(node.Text) && node.Children.Count == 0)
            {
                string preview = node.Text.Length > 40 ? node.Text.Substring(0, 37) + "..." : node.Text;
                preview = preview.Replace("\r", "").Replace("\n", "\\n");
                Console.WriteLine($"{indentation}    \"{preview}\"");
            }

            foreach (var child in node.Children)
            {
                PrintNode(child, indent + 1);
            }
        }

        static void PrintRawNode(Node node, int indent)
        {
            string indentation = new string(' ', indent * 2);
            string fieldName = node.Parent?.GetFieldNameForChild(node.Parent.Children.ToList().IndexOf(node)) ?? "";
            string fieldPrefix = !string.IsNullOrEmpty(fieldName) ? $"{fieldName}: " : "";

            Console.WriteLine($"{indentation}- {fieldPrefix}{node.Type} [{node.StartPosition.Row}:{node.StartPosition.Column}]");

            foreach (var child in node.Children)
            {
                PrintRawNode(child, indent + 1);
            }
        }    }
}
