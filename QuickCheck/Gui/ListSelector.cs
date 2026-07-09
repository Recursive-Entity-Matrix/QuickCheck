using Dalamud.Bindings.ImGui;
using OtterGui;
using QuickCheck.Models;

namespace QuickCheck.Gui;

public class ListSelector : ItemSelector<CheckList>
{
    private readonly Config _config;
    public ListSelector(Config config) : base(config.CheckLists, Flags.Add | Flags.Delete | Flags.Filter)
    {
        _config = config;
    }
    
    protected override bool Filtered(int idx) => Filter.Length != 0 &&
                                                 !Items[idx].Name.Contains(Filter,
                                                     StringComparison.InvariantCultureIgnoreCase);
    
    protected override bool OnDraw(int idx)
    {
        return ImGui.Selectable(Items[idx].Name, CurrentIdx == idx);
    }

    protected override bool OnDelete(int idx)
    {
        if (idx < 0 || idx >= Items.Count)
            return false;
        
        Items.RemoveAt(idx);
        _config.Save();
        return true;
    }

    protected override bool OnAdd(string name)
    {
        Items.Add(new CheckList {Name = name});
        _config.Save();
        return true;
    }
}