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
        
        var targetName = target.TargetName;
        var targetHomeWorld  = target.TargetHomeWorld.RowId;
        if (string.IsNullOrEmpty(targetName) || targetHomeWorld == 0) return;
        
        var checkItem = new MenuItem()
        {
            Name = seString,
            UseDefaultPrefix = false,
            PrefixChar = 'Q',
            PrefixColor = 526,
            OnClicked = (a) =>
            {
                var found = _config.CheckLists.FirstOrDefault(x => x.VipPlayers.Any(v => v.Name == targetName && v.HomeWorld == targetHomeWorld));
                if (found != null)
                {
                    Services.ChatGui.Print($"[Quick Check] {targetName} is on the VIP list! ({found.Name})");
                }
                else
                {
                    Services.ChatGui.PrintError($"[Quick Check] {targetName} is not any VIP list!");
                }
            }
        };
        args.AddMenuItem(checkItem);
    }

    public void Dispose()
    {
        
    }
}