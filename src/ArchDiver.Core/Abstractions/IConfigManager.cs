using ArchDiver.Core.Models;

namespace ArchDiver.Core.Abstractions;

public interface IConfigManager
{
    ProjectConfig Load(string path);
    void Save(ProjectConfig config, string path);
    ProjectConfig GetDefault();
}
