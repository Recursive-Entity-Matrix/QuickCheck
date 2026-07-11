using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using QuickCheck.Gui;
using QuickCheck.Models;

namespace QuickCheck;

public class QuickCheck : IDalamudPlugin
{
    private readonly Config _config;
    private readonly WindowSystem _windowSystem = new("QuickCheck");
    private readonly MainWindow _mainWindow;
    private readonly ConfigWindow _configWindow;

    public QuickCheck(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Services>();
        _config = Config.Load();
        _mainWindow = new MainWindow(_config);
        _configWindow = new ConfigWindow(_config);
        _windowSystem.AddWindow(_mainWindow);
        _windowSystem.AddWindow(_configWindow);
        Services.PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        Services.PluginInterface.UiBuilder.OpenMainUi += () => _mainWindow.Toggle();
        Services.PluginInterface.UiBuilder.OpenConfigUi += () => _configWindow.Toggle();
        Services.ContextMenu.OnMenuOpened += ContextMenu_OnMenuOpened;

        Services.CommandManager.AddHandler("/quickcheck", new CommandInfo(OnCommand)
        {
            HelpMessage = "config > Open Configuration Menu\nnow > Check current target\n(none) > Open Main Menu"
        });

        if (_config.UpdateOnStartup)
        {
            foreach (var list in _config.CheckLists)
            {
                Task.Run(list.UpdateCheckList);
            }
        }
        
        Services.Framework.Update += Framework_Update;
    }

    private DateTime _lastUpdate = DateTime.Now;
    private void Framework_Update(IFramework framework)
    {
        if (!((DateTime.Now - _lastUpdate).TotalMinutes >= _config.AutoUpdateMinutes)) return;
        _lastUpdate = DateTime.Now;
        foreach (var list in _config.CheckLists)
        {
            if (list.ProviderType == ProviderType.None) continue;
            Services.Log.Debug($"Auto updating list {list.Name} from {list.ProviderType}");
            Task.Run(list.UpdateCheckList);
        }
    }
    
    private void OnCommand(string command, string arguments)
    {
        if (string.IsNullOrEmpty(arguments))
        {
            _mainWindow.Toggle();
            return;
        }

        if (string.Equals(arguments, "now"))
        {
            if (Services.TargetManager.Target is not IPlayerCharacter target)
            {
                Services.ChatGui.PrintError("[Quick Check] Please target a player.");
                return;
            }

            RunCheck(target);
            return;
        }

        if (string.Equals(arguments, "config"))
        {
            _configWindow.Toggle();
        }

        Services.ChatGui.PrintError($"[Quick Check] '{arguments}' is not a valid command!");
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
            OnClicked = (a) => { RunCheck(pc); }
        };
        args.AddMenuItem(checkItem);
    }

    public void RunCheck(IPlayerCharacter pc)
    {
        var found = _config.CheckLists.Where(x =>
            x.VipPlayers.Any(v => v.Name == pc.Name.TextValue && v.HomeWorld == pc.HomeWorld.RowId)).ToList();
        if (found.Count > 0)
        {
            Services.ChatGui.Print(
                $"[Quick Check] {pc.Name.TextValue} is on the VIP list! ({string.Join(", ", found.Select(x => x.Name))})");
            if (_config.PlayChatSounds)
                UIGlobals.PlayChatSoundEffect(_config.SuccessSound);
        }
        else
        {
            Services.ChatGui.PrintError($"[Quick Check] {pc.Name.TextValue} is not on any VIP list!");
            if (_config.PlayChatSounds)
                UIGlobals.PlayChatSoundEffect(_config.ErrorSound);
        }
    }

    public void Dispose()
    {
    }
}