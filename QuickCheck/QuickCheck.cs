using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using QuickCheck.Gui;

namespace QuickCheck;

public class QuickCheck : IDalamudPlugin
{
    private readonly Config _config;
    private readonly WindowSystem _windowSystem = new("QuickCheck");
    private readonly MainWindow _mainWindow;
    public QuickCheck(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Services>();
        _config = Config.Load();
        _mainWindow = new MainWindow(_config);
        _windowSystem.AddWindow(_mainWindow);
        Services.PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        Services.PluginInterface.UiBuilder.OpenMainUi += () => _mainWindow.Toggle();
        Services.ContextMenu.OnMenuOpened += ContextMenu_OnMenuOpened;
    }

    private void ContextMenu_OnMenuOpened(IMenuOpenedArgs args)
    {
        if (args.MenuType == ContextMenuType.Inventory) return;
        if (args.Target is not MenuTargetDefault target) return;
        
        SeStringBuilder builder = new();
        var seString = builder.AddText("Check The List").Build();

        var obj = Services.Objects.SearchById(target.TargetObjectId);
        if (obj is not IPlayerCharacter pc) return;
        
        var checkItem = new MenuItem()
        {
            Name = seString,
            UseDefaultPrefix = false,
            PrefixChar = 'Q',
            PrefixColor = 526,
            OnClicked = (a) =>
            {
                var found = _config.CheckLists.Where(x => x.VipPlayers.Any(v => v.Name == pc.Name.TextValue && v.HomeWorld == pc.HomeWorld.RowId)).ToList();
                if (found.Count > 0)
                {
                    Services.ChatGui.Print($"[Quick Check] {pc.Name.TextValue} is on the VIP list! ({string.Join(", ", found.Select(x => x.Name))})");
                }
                else
                {
                    Services.ChatGui.PrintError($"[Quick Check] {pc.Name.TextValue} is not on any VIP list!");
                }
            }
        };
        args.AddMenuItem(checkItem);
    }

    public void Dispose()
    {
        
    }
}