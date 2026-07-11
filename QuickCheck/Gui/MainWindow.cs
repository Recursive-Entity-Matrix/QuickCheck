using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using OtterGui;
using OtterGui.Raii;
using QuickCheck.Models;
using QuickCheck.Util;

namespace QuickCheck.Gui;

public class MainWindow : Window
{
    private readonly Config _config;
    private readonly ListSelector _listSelector;
    public MainWindow(Config config) : base($"Quick Check {Assembly.GetExecutingAssembly().GetName().Version}")
    {
        _config = config;
        _listSelector = new ListSelector(_config);
        Size = new Vector2(800, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        _listSelector.Draw(ImGui.GetContentRegionAvail().X * 0.25f);
        ImGui.SameLine();
        if (_listSelector.Current == null)
        {
            ImGui.Text("No list selected");
            return;
        }
        DrawSelectedList(_listSelector.Current);
    }

    private void DrawSelectedList(CheckList list)
    {
        using var child = ImRaii.Child("SelectedList");
        
        var name = list.Name;
        if (ImGui.InputText("Name", ref name, 100))
        {
            list.Name = name;
            _config.Save();
        }
        
        var description = list.Description;
        if (ImGui.InputTextMultiline("Description", ref description, 1000, new Vector2(0, 60)))
        {
            list.Description = description;
            _config.Save();
        }
        
        if (ImGuiUtil.GenericEnumCombo("Remote Provider", 225f, list.ProviderType, out var newProvider, ProviderTypeHelper.GetProviderPretty))
        {
            list.ProviderType = newProvider;
            _config.Save();
        }
        ImGuiUtil.HoverTooltip(@"Connect the list to a remote provider to sync the list between multiple people." + Environment.NewLine
                              + "If using a Google Sheet, the sheet must contain a column named 'Player Name'. Additional columns are allowed and will not be parsed." + Environment.NewLine
                              + "If using a Google Doc, the doc must contain a list of player names, one per line."  + Environment.NewLine
                              + "Both must have the names listed in the format 'Player Name@World'" + Environment.NewLine
                              + "Example: Thancred Waters@Kraken" + Environment.NewLine
                              + "NOTE: Using this option will disable the ability to edit the list locally.");
        var hasRemoteProvider = list.ProviderType != ProviderType.None;
        if (hasRemoteProvider)
        {
            var url = list.ProviderUrl;
            if (ImGui.InputText("Url", ref url, 500))
            {
                list.ProviderUrl = url;
                _config.Save();
            }

            if (ImGui.Button("Sync Remote Provider"))
            {
                Task.Run(list.UpdateCheckList);
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Remote Provider"))
            {
                if (!string.IsNullOrEmpty(list.ProviderUrl))
                    Dalamud.Utility.Util.OpenLink(list.ProviderUrl);
            }
        }
        else
        {
            if (ImGui.Button("Add New VIP"))
            {
                list.VipPlayers.Add(new VIPPlayer());
            }
        }

        DrawVIPList(list, hasRemoteProvider);
    }

    private VIPPlayer? _selectedRemovePlayer;
    private void DrawVIPList(CheckList list, bool hasRemoteProvider)
    {
        using var child = ImRaii.Child("VIPList");
        var clipper = new ImGuiListClipper();
        clipper.Begin(list.VipPlayers.Count);
        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                if (hasRemoteProvider)
                    ImGui.BeginDisabled();
                var player = list.VipPlayers[i];
                using var playerId = ImRaii.PushId(i);
                var name = player.Name;
                if (ImGui.InputText("Name", ref name, 100))
                {
                    player.Name = name;
                    _config.Save();
                }
                var worldNames = ServerHelper.Worlds.Select(x => x.Name.ToString()).ToList();
                
                int homeWorldIndex = ServerHelper.Worlds.FindIndex(x => x.RowId == player.HomeWorld);
                if (homeWorldIndex < 0)
                    homeWorldIndex = 0;

                if (ImGui.Combo("Home World", ref homeWorldIndex, worldNames))
                {
                    if (homeWorldIndex >= 0 && homeWorldIndex < ServerHelper.Worlds.Count)
                    {
                        player.HomeWorld = ServerHelper.Worlds[homeWorldIndex].RowId;
                        _config.Save();
                    }
                }

                if (ImGui.Button("Remove"))
                {
                    _selectedRemovePlayer = player;
                }
                if (hasRemoteProvider)
                    ImGui.EndDisabled();
                ImGui.Separator();
            }
        }

        if (_selectedRemovePlayer != null)
        {
            list.VipPlayers.Remove(_selectedRemovePlayer);
            _config.Save();
            _selectedRemovePlayer = null;
        }
    }
}