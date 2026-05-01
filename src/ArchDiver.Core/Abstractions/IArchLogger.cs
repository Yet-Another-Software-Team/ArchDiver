namespace ArchDiver.Core.Abstractions;

public interface IArchLogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
}
