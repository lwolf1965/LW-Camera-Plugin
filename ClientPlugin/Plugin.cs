using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClientPlugin.Settings;
using ClientPlugin.Tools;
using HarmonyLib;
using Keen.VRage.Core.Plugins;
using Keen.VRage.Library.Diagnostics;

namespace ClientPlugin;

public class Plugin : IPlugin
{
    public const string Name = "LwCameraPlugin";
    /// <summary>
    /// Plugin version, the only place to change it (AssemblyInfo.cs uses it). Scheme: STAGE.FEATURE.FIX.BUILD where
    /// STAGE follows the game: 0.1 = game alpha, 0.2 = game beta, 1 = game released.
    /// </summary>
    public const string Version = "0.1.0.0";
    public static Plugin Instance;

    // The data directory will be provided by a proper SDK in the future.
    // This static function is currently injected by Pulsar, which will
    // remain compatible, even after the SDK's release.
#pragma warning disable CS0649 // This field is assigned by Pulsar
    private static Func<string, string, string> GetConfigPath;
#pragma warning restore CS0649
    public string DataDir { get; private set; } = GetConfigPath(Name, null);

    /// <summary>Patches the plugin can run without, and what happens then.</summary>
    private static readonly Dictionary<Type, string> OptionalPatches = new()
    {
        [typeof(Patches.TurretAimPatch)] = "zoomed turret aim stays at normal speed",
        [typeof(Patches.ControlsFollowCameraPatch)] = "controls stay as in the cockpit",
        [typeof(Patches.EscapePatch)] = "Esc opens the menu as usual",
        [typeof(Patches.WorldUnloadPatch)] = "world references are dropped at the next world load",
    };

    public Plugin()
    {
        Instance = this;

        // Force-load Config.Current now that DataDir is available.
        _ = Config.Current;

        Game.PluginLog.Info($"Loaded {Name} {Version}. Enabled={Config.Current.Enabled}");

        // Patch each class separately so one broken hook is logged instead of stopping the plugin load.
        var harmony = new Harmony(Name);
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!type.GetCustomAttributes<HarmonyPatch>().Any()) continue;
            try
            {
                harmony.CreateClassProcessor(type).Patch();
                Game.PluginLog.Info($"Patched {type.Name}");
            }
            catch (Exception e)
            {
                // Only the main loop is essential; without an optional patch one feature falls back to the game's behaviour.
                if (OptionalPatches.TryGetValue(type, out var fallback))
                {
                    Game.PluginLog.Warn($"Patch {type.Name} failed, {fallback}: {e.GetBaseException().Message}");
                    continue;
                }
                Game.PluginLog.Error($"Patch {type.Name} failed, plugin disabled: {e}");
                Game.CameraEngine.Disable("patch failed: " + type.Name);
            }
        }
    }

    // Invoked by Pulsar via reflection when the user clicks the plugin's config button.
    public void OpenConfigDialog()
    {
        try
        {
            var sharedUi = GameAccess.GetSharedUI();
            if (sharedUi == null)
            {
                Log.Default.WriteLine(LogSeverity.Warning, $"[{Name}] SharedUIComponent not available");
                return;
            }

            var generator = new SettingsGenerator();
            var viewModel = new SettingsScreenViewModel(
                generator.Title,
                panel => generator.PopulateContent(panel),
                () => ConfigStorage.Save(Config.Current));

            sharedUi.CreateScreen<SettingsScreen>(viewModel, showCursor: true);
        }
        catch (Exception e)
        {
            Log.Default.WriteLine(LogSeverity.Error, $"[{Name}] OpenConfigDialog failed: {e}");
        }
    }
}
