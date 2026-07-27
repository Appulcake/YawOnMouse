using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using YawOnMouse.Blacklist;
using InputFramework;

namespace YawOnMouse;

public static class PluginInfo
{
    public const string PLUGIN_GUID = "YawOnMouse";
    public const string PLUGIN_NAME = "YawOnMouse";
    public const string PLUGIN_VERSION = "2.1.0";
}

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    public static ConfigEntry<bool> Enabled;
    public static ConfigEntry<AxisPatchType> AxisPatchType;
    public static ConfigEntry<bool> UseCraftWhitelist;
    public WhitelistConfigManager WhitelistConfigManager;
    public static Plugin Instance;
    
    private const string ACTION_TOGGLE = "Yaw On Mouse Toggle";
    public static bool PilotInControl = false;

    private bool _scanComplete = false;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;
        WhitelistConfigManager = new WhitelistConfigManager();

        Enabled = Config.Bind(
            "Config",
            "PlayerAxisControls_Patch",
            true,
            "Enable/Disable controller roll input patch");
        AxisPatchType = Config.Bind(
            "Config",
            "AxisPatchType",
            YawOnMouse.AxisPatchType.Yaw,
            "What you want the patch to do on the x-axis (can only be changed before startup not at runtime.)"
        );
        UseCraftWhitelist = Config.Bind(
            "Config",
            "UseCraftWhitelist",
            false,
            "When enabled the mod will only work on the aircraft specified in the whitelist"
            );
        
        ExtraInputManager.LoadPendingActions();
        ExtraInputManager.RegisterAction(ACTION_TOGGLE, Rewired.InputActionType.Button, "Flight");
        
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }

    private void Update()
    {
        if (PilotInControl)
        {
            var player = ReInput.players?.GetPlayer(0);
            if (player != null && player.GetButtonDown(ACTION_TOGGLE))
            {
                Plugin.Logger.LogInfo("Detected yaw toggle");
                Enabled.Value = !Enabled.Value;
                Config.Save();
#if DEBUG
                Logger.LogInfo($"Plugin toggled: {(Enabled.Value ? "Enabled" : "Disabled")}");
#endif
            }
        }
        
        // really dirty ik, but only runs when plugin is first ran
        if (!_scanComplete)
        {
            _scanComplete = WhitelistConfigManager.TryScanForAircraft();
        }
    }
}