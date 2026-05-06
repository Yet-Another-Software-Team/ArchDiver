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
        public List<string> IgnorePatterns { get; set; } = [".git", "bin", "obj", ".archdiver"];
        public int MaxDepth { get; set; } = 10;
    }
}
