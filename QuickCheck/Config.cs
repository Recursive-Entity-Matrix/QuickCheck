using Dalamud.Configuration;
using Newtonsoft.Json;
using QuickCheck.Models;

namespace QuickCheck;

public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public List<CheckList> CheckLists { get; set; } = [];

    public void Save()
    {
        var json = JsonConvert.SerializeObject(this);
        var path = Services.PluginInterface.ConfigFile.FullName;
        File.WriteAllText(path, json);
    }

    public static Config Load()
    {
        var path = Services.PluginInterface.ConfigFile.FullName;
        if (!File.Exists(path))
        {
            return new Config();
        }
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<Config>(json) ??  new Config();
    }
}