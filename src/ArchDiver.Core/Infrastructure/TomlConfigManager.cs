using Tomlyn;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Infrastructure;

public class TomlConfigManager : IConfigManager
{
    public ProjectConfig Load(string path)
    {
        if (!File.Exists(path)) return GetDefault();

        string toml = File.ReadAllText(path);
        return TomlSerializer.Deserialize<ProjectConfig>(toml) ?? GetDefault();
    }

    public void Save(ProjectConfig config, string path)
    {
        var options = new TomlSerializerOptions { WriteIndented = true };
        string toml = TomlSerializer.Serialize(config, options);
        File.WriteAllText(path, toml);
    }

    public ProjectConfig GetDefault() => new ProjectConfig();
}
