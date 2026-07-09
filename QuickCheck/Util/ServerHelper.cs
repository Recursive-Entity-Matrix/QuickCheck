using Lumina.Excel.Sheets;

namespace QuickCheck.Util;

public static class ServerHelper
{
    static ServerHelper()
    {
        WorldsById = Services.Data.GameData.GetExcelSheet<World>()?.ToDictionary(x => x.RowId, x => x.Name.ToString()) ?? new Dictionary<uint, string>();
    }

    private static readonly Dictionary<uint, string> WorldsById;
    
    public static List<string> GetWorlds() => WorldsById.Values.ToList();
    
    public static string GetNameFromId(uint id) => WorldsById.GetValueOrDefault(id, "Unknown");
    public static uint GetIdFromName(string name) => WorldsById.FirstOrDefault(x => x.Key.ToString().Equals(name, StringComparison.OrdinalIgnoreCase)).Key;
}