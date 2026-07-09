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
                //TODO
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
                //TODO
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
                var player = list.VipPlayers[i];
                using var playerId = ImRaii.PushId(i);
                var name = player.Name;
                if (ImGui.InputText("Name", ref name, 100))
                {
                    player.Name = name;
                    _config.Save();
                }
                ImGui.SameLine();
                int homeWorld = (int)player.HomeWorld;
                if (ImGui.Combo("Home World", ref homeWorld, ServerHelper.GetWorlds()))
                {
                    player.HomeWorld = (uint)homeWorld;
                    _config.Save();
                }

                if (ImGui.Button("Remove"))
                {
                    _selectedRemovePlayer = player;
                }
                
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