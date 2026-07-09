using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace QuickCheck.Util;

public static class ServerHelper
{
    static ServerHelper()
    {
        Worlds = Services.Data.GameData.GetExcelSheet<World>()?.Where(x => x.IsPublic).ToList()!;
    }

    public static readonly List<World> Worlds;
    
    public static string GetNameFromId(uint id) => Worlds.FirstOrNull(x => x.RowId == id).ToString() ?? string.Empty;
    public static uint GetIdFromName(string name) => Worlds.FirstOrNull(x => x.Name.ToString().Equals(name, StringComparison.OrdinalIgnoreCase))?.RowId ?? 0;
}