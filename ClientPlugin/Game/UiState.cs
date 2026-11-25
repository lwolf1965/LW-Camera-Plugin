using System;
using System.Collections.Generic;
using ClientPlugin.Tools;
using HarmonyLib;
using Keen.Game2.Client.UI.Library;
using Keen.VRage.UI.Screens;

namespace ClientPlugin.Game;

/// <summary>
/// Tells whether a screen with a mouse cursor is open (terminal, inventory, menus, the plugin settings).
///
/// Game API: SharedUIComponent keeps the screens that show the cursor in the private set
/// _cursorVisibleScreenHandles (SharedUIComponent.UpdateCursorVisibility shows the cursor while it is not empty).
/// Read through a typed Harmony field reference, created once; if the field is renamed in a game update the check is
/// switched off and logged once.
/// </summary>
internal static class UiState
{
    private static AccessTools.FieldRef<SharedUIComponent, HashSet<ScreenHandle>> cursorScreens;
    private static SharedUIComponent sharedUi;
    private static bool unavailable;

    public static bool IsCursorScreenOpen()
    {
        if (unavailable) return false;
        try
        {
            sharedUi ??= GameAccess.GetSharedUI();
            if (sharedUi == null) return false;

            cursorScreens ??= AccessTools.FieldRefAccess<SharedUIComponent, HashSet<ScreenHandle>>("_cursorVisibleScreenHandles");
            return cursorScreens(sharedUi).Count > 0;
        }
        catch (Exception e)
        {
            unavailable = true;
            PluginLog.Warn("Cannot detect open screens (game changed?), keys and wheel stay active while menus are open: " + e.Message);
            return false;
        }
    }
}
