namespace ArchDiver.Core.Models;

public class ProjectConfig
{
    public LoggingConfig Logging { get; set; } = new();
    public AnalysisConfig Analysis { get; set; } = new();

    public class LoggingConfig
    {
        public int Level { get; set; } = 1; // 0: Error, 1: Warning, 2: Info
    }

    public class AnalysisConfig
    {
        public List<string> IgnorePatterns { get; set; } = [".git", "bin", "obj", ".archdiver"];
        public int MaxDepth { get; set; } = 10;
    }
}
