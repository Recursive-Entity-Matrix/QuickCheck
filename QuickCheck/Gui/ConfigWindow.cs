using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using OtterGui;

namespace QuickCheck.Gui;

public class ConfigWindow : Window
{
    private readonly Config _config;
    public ConfigWindow(Config config) : base("Quick Check Configuration")
    {
        _config = config;
        Size = new Vector2(400, 300);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var updateOnStartup =  _config.UpdateOnStartup;
        if (ImGui.Checkbox("Update On Startup", ref updateOnStartup))
        {
            _config.UpdateOnStartup = updateOnStartup;
            _config.Save();
        }
        ImGuiUtil.HoverTooltip("Automatically update all remote lists when the plugin is loaded.");
        var autoUpdateMinute = _config.AutoUpdateMinutes;
        if (ImGui.InputUInt("Auto Update Minutes", ref autoUpdateMinute))
        {
            _config.AutoUpdateMinutes = autoUpdateMinute;
            _config.Save();
        }
        ImGuiUtil.HoverTooltip("Update all remote lists on an interval.");
        var sounds = _config.PlayChatSounds;
        if (ImGui.Checkbox("Play Chat Sounds", ref sounds))
        {
            _config.PlayChatSounds = sounds;
            _config.Save();
        }
        ImGuiUtil.HoverTooltip("Play a sound effect when checking the list.\n(Sounds only play for you)\n(Valid values are 1-16)");
        if (sounds)
        {
            var successSound = _config.SuccessSound;
            if (ImGui.InputUInt("Found Sound", ref successSound))
            {
                if (successSound < 1)
                    successSound = 1;
                if (successSound > 16)
                    successSound = 16;
                _config.SuccessSound = successSound;
                _config.Save();
            }

            var failSound = _config.ErrorSound;
            if (ImGui.InputUInt("Not Found Sound", ref failSound))
            {
                if (failSound < 1)
                    failSound = 1;
                if (failSound > 16)
                    failSound = 16;
                _config.ErrorSound = failSound;
                _config.Save();
            }
        }
    }
}