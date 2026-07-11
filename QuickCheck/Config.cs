using Dalamud.Configuration;
using Newtonsoft.Json;
using QuickCheck.Models;

namespace QuickCheck;

public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public List<CheckList> CheckLists { get; set; } = [];
    public bool PlayChatSounds { get; set; } = true;
    public uint SuccessSound { get; set; } = 3;
    public uint ErrorSound { get; set; } = 11;
    public uint AutoUpdateMinutes {get; set;} = 15;
    public bool UpdateOnStartup { get; set; } = true;

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
        return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
    }
}