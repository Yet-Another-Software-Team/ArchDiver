using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Tomlyn;
using ArchDiver.Core;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Models;
using ArchDiver.Core.Parsing;
using ArchDiver.Core.Abstractions;
using TreeSitter;

namespace ArchDiver.Cli
{
    class Program
    {
        private static int _maxDepth = 10;
        private static string _outputDir = ".archdiver/out";

        static void Main(string[] args)
        {
            // Initialize the microkernel
            Bootstrapper.Initialize();

            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }

            string command = args[0].ToLower();

            switch (command)
            {
                case "explore":
                    HandleExplore(args);
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
            Console.WriteLine("  archdiver explore <directory_path> [--max-depth <n>]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  explore  Recursively parses all supported files in a directory.");
            Console.WriteLine();
            Console.WriteLine($"Supported Languages: {string.Join(", ", LanguageRegistry.GetSupportedLanguages())}");
        }

        static void HandleExplore(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Missing directory path.");
                return;
            }

            string rootPath = Path.GetFullPath(args[1]);
            if (!Directory.Exists(rootPath))
            {
                Console.WriteLine($"Error: Directory not found: {rootPath}");
                return;
            }

            // Parse max-depth
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--max-depth" && i + 1 < args.Length && int.TryParse(args[i + 1], out int depth))
                {
                    _maxDepth = depth;
                }
            }

            Console.WriteLine($"Exploring directory: {rootPath} (Max Depth: {_maxDepth})");

            string outputRoot = Path.Combine(rootPath, _outputDir);
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, true);
            }
            Directory.CreateDirectory(outputRoot);

            ExploreDirectory(rootPath, rootPath, outputRoot, 0);

            Console.WriteLine($"Exploration complete. Results saved in {outputRoot}");
        }

        static void ExploreDirectory(string rootPath, string currentPath, string outputRoot, int depth)
        {
            if (depth > _maxDepth) return;

            // Skip output directory
            if (currentPath.Replace("\\", "/").Contains("/.archdiver/out")) return;

            try
            {
                foreach (var file in Directory.GetFiles(currentPath))
                {
                    ProcessFile(rootPath, file, outputRoot);
                }

                foreach (var dir in Directory.GetDirectories(currentPath))
                {
                    ExploreDirectory(rootPath, dir, outputRoot, depth + 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to access {currentPath}: {ex.Message}");
            }
        }

        static void ProcessFile(string rootPath, string filePath, string outputRoot)
        {
            try
            {
                string sourceCode = File.ReadAllText(filePath);
                var provider = LanguageRegistry.Identify(filePath, sourceCode);

                if (provider == null) return; // Skip unsupported files

                Console.WriteLine($"Parsing: {Path.GetRelativePath(rootPath, filePath)} ({provider.LanguageId})");

                var codeParser = new CodeParser(provider);
                var ast = codeParser.Parse(sourceCode);

                var conceptExtractor = new ConceptExtractor(provider);
                var analysisResult = conceptExtractor.Extract(ast);

                string relativePath = Path.GetRelativePath(rootPath, filePath);
                string analysisOutputPath = Path.Combine(outputRoot, relativePath + ".concepts.toml");

                Directory.CreateDirectory(Path.GetDirectoryName(analysisOutputPath)!);

                var options = new TomlSerializerOptions
                {
                    WriteIndented = true,
                    IndentSize = 4,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                // Save Analysis
                var analysisTomlModel = ToTomlModel(analysisResult);
                string analysisToml = TomlSerializer.Serialize(analysisTomlModel, options);
                File.WriteAllText(analysisOutputPath, analysisToml);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {filePath}: {ex.Message}");
            }
        }

        static Tomlyn.Model.TomlTable ToTomlModel(LanguageAnalysisResult result)
        {
            var table = new Tomlyn.Model.TomlTable();
            table["language"] = result.Language;

            table["methods"] = ToTomlArray(result.Methods);
            table["classes"] = ToTomlArray(result.Classes);
            table["fields"] = ToTomlArray(result.Fields);
            table["imports"] = ToTomlArray(result.Imports);
            table["identifiers"] = ToTomlArray(result.Identifiers);

            return table;
        }

        static Tomlyn.Model.TomlTableArray ToTomlArray(List<ExtractedConcept> concepts)
        {
            var array = new Tomlyn.Model.TomlTableArray();
            foreach (var concept in concepts)
            {
                var table = new Tomlyn.Model.TomlTable();
                table["type"] = concept.Type;
                table["text"] = concept.Text;

                var rangeTable = new Tomlyn.Model.TomlTable();
                var startTable = new Tomlyn.Model.TomlTable();
                startTable["line"] = concept.Range.Start.Line;
                startTable["column"] = concept.Range.Start.Column;
                rangeTable["start"] = startTable;

                var endTable = new Tomlyn.Model.TomlTable();
                endTable["line"] = concept.Range.End.Line;
                endTable["column"] = concept.Range.End.Column;
                rangeTable["end"] = endTable;

                table["range"] = rangeTable;
                array.Add(table);
            }
            return array;
        }

        static void PrintNode(AstNode node, int indent)
        {
            string indentation = new string(' ', indent * 2);
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
    }
}
