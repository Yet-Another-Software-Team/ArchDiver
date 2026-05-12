using Microsoft.Extensions.Logging;

namespace ArchDiver.Core.Models;

public class ProjectConfig
{
    public LoggingConfig Logging { get; set; } = new();
    public AnalysisConfig Analysis { get; set; } = new();

    public class LoggingConfig
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
    }

    public class AnalysisConfig
    {
        // Standard glob patterns like .gitignore
        public List<string> IgnorePatterns { get; set; } =
        [
            "**/.git/**",
            "**/bin/**",
            "**/obj/**",
            "**/.archdiver/**",
            "**/*.exe",
            "**/*.dll",
            "**/*.pdb"
        ];
        public int MaxDepth { get; set; } = 10;
        public double ConfidenceThreshold { get; set; } = 0.75;
    }
}
