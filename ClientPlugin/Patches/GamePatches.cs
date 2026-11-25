using ClientPlugin.Game;
using HarmonyLib;
using Keen.Game2.Client.GameSystems.PlayerControl.PlayerInput.InputHandlers;
using Keen.Game2.Client.UI.InGame;
using Keen.Game2.Simulation.WorldObjects.Movement;
using Keen.VRage.DCS.Accessors;
using Keen.VRage.Library.Mathematics;
using Keen.VRage.Library.Utils;
using Keen.VRage.Core.Game.Systems;

namespace ClientPlugin.Patches;

/// <summary>
/// Main loop: runs the camera logic after each Session.Update(bool updateLifetime).
/// The camera itself needs no patch: CameraComponent.SetTransformOverride / SetCustomFOV are public
/// game API, and the override is applied by the game after the camera controllers and before render.
/// </summary>
[HarmonyPatch(typeof(Session), nameof(Session.Update), new[] { typeof(bool) })]
internal static class SessionUpdatePatch
{
    // Session.Update(false) is an extra call the game makes outside the normal frame loop (e.g. while saving).
    // Running on it too would read the same mouse-wheel value twice.
    private static void Postfix(Session __instance, bool doEntityLifetimeUpdates)
    {
        if (doEntityLifetimeUpdates) CameraEngine.OnUpdate(__instance);
    }
}

/// <summary>
/// World unload: lets the plugin drop its references into the client world before the game disposes it.
/// Game method: SessionInGameUISessionComponent.OnBeforeSessionUnloaded() — Session.Dispose() calls it on every
/// session component first, while the camera still exists. (Session.Dispose itself cannot be patched: Harmony
/// rejects it, it contains exception filters.) This component exists only in the client session.
/// </summary>
[HarmonyPatch(typeof(SessionInGameUISessionComponent), "OnBeforeSessionUnloaded")]
internal static class WorldUnloadPatch
{
    private static void Prefix() => CameraEngine.OnWorldUnloading();
}

/// <summary>
/// Turret aim while zoomed: the game sends the raw mouse movement to the turret, so a zoomed view would swing as
/// fast as an unzoomed one. Scales it by the zoom (same formula as aim-down-sights). No effect when not zoomed.
/// Game method: AutomatedWeaponInputHandlerComponent.HandleMouseLookAnalog(Vector2 control, ControlActivation).
/// </summary>
[HarmonyPatch(typeof(AutomatedWeaponInputHandlerComponent), "HandleMouseLookAnalog")]
internal static class TurretAimPatch
{
    private static void Prefix(ref Vector2 control)
    {
        var sensitivity = CameraEngine.TurretAimSensitivity;
        if (sensitivity < 1f) control *= sensitivity;
    }
}

/// <summary>
/// Controls follow the dock camera. The cockpit hands its inputs to the grid with its own orientation in the grid;
/// while the dock camera is on, the camera's orientation is used instead (see CameraControls).
/// Game method: EntityMovementExtensions.UpdateControlData(this in DEntityContext, in MovementInputs,
/// in Quaternion? rotation, bool isAngular). Also called for characters: CameraControls only acts on the grid
/// the dock camera belongs to.
/// </summary>
[HarmonyPatch(typeof(EntityMovementExtensions), nameof(EntityMovementExtensions.UpdateControlData))]
internal static class ControlsFollowCameraPatch
{
    // Both parameters are "in" in the game method (passed by reference); Harmony binds them by name as ref.
    private static void Prefix(ref DEntityContext data, ref Quaternion? rotation)
    {
        CameraControls.Apply(in data, ref rotation);
    }
}

/// <summary>
/// Esc leaves the dock camera or the controlled turret instead of opening the in-game menu (a second Esc opens it).
/// Game method: SessionInGameUISessionComponent.CreateInGameMenuScreen(), called by HotKeysSessionComponent for the
/// MenuScreen key (Esc). Skipping it returns null, which the hotkey ignores.
/// </summary>
[HarmonyPatch(typeof(SessionInGameUISessionComponent), nameof(SessionInGameUISessionComponent.CreateInGameMenuScreen))]
internal static class EscapePatch
{
    private static bool Prefix(ref IObservableDisposable __result)
    {
        if (!CameraEngine.HandleEscape()) return true;
        __result = null;
        return false;
    }
}
