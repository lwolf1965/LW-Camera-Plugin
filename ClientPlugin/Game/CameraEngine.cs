using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClientPlugin.Policy;
using Keen.Game2.Client.GameSystems.CameraSystems;
using Keen.Game2.Client.UI.InGame;
using Keen.Game2.Client.WorldObjects.CubeBlocks.Weapons;
using Keen.Game2.Simulation.WorldObjects.CubeBlocks;
using Keen.VRage.Core.Game.Components;
using Keen.VRage.Core.Game.Systems;
using Keen.VRage.DCS.Components;
using Keen.VRage.Library.Utils;

namespace ClientPlugin.Game;

/// <summary>
/// The camera logic, run once per frame from Session.Update.
///
/// Two features, nothing else:
///  - Dock camera: a fixed view out of a connector of the ship you fly (no mouse look, wheel zoom).
///  - Turret camera: while the player controls a turret (game's own turret control), the view is on the barrel;
///    the mouse aims through the game, the wheel zooms.
///
/// Camera view: CameraComponent.SetTransformOverride(pose) every frame. The game applies the override after
/// the camera controllers and before render (CameraComponent.OverrideCamera, job group OnCameraTranformOverride).
/// Zoom: CameraComponent.SetCustomFOV(radians, mouseSensitivity) / ResetCustomFOV() (used by aim-down-sights).
/// Enter/exit: CameraComponent.SetNextTransitionNonSmooth() so the view jumps instead of sliding.
/// Ship control: the player's own camera input is NOT locked. CameraSystemComponent.EnableCameraInput(false) puts a
/// DisabledTag on the active camera controller, and in a cockpit that can be the seat: SeatComponent.IsEnabled then
/// turns false and the cockpit ignores W A S D C Space and the mouse. The override replaces the view anyway.
/// Controls in the camera's axes: CameraControls + ControlsFollowCameraPatch.
/// </summary>
internal static class CameraEngine
{
    private static readonly InputReader Input = new();

    private static bool failed;
    private static bool failedUntilRestart;
    private static int errorCount;
    private static long lastErrorTicks;
    private static WeakReference<Session> clientSession;

    // Camera view state
    private static bool dockActive;
    private static List<CameraPoint> dockPoints = new();
    private static int dockIndex;
    private static Entity dockGridEntity;
    private static CameraComponent dockRenderCamera;
    private static float dockFovDegrees;
    private static readonly Dictionary<Entity, int> LastDockIndexPerGrid = new();

    // Turret camera state
    private static TurretCamera barrelView;
    private static Entity turretBlock;
    private static AutomatedWeaponBlockToolControllableComponent turretControl;
    private static CameraComponent turretRenderCamera;
    private static float turretFovDegrees;
    private static float turretNormalFovDegrees;
    private static bool turretBarrelViewSetting;
    private static bool turretReleaseRequested;

    /// <summary>Mouse aim multiplier for the turret while zoomed (1 = not zoomed). Read by TurretAimPatch.</summary>
    public static float TurretAimSensitivity { get; private set; } = 1f;

    /// <summary>
    /// Stops the plugin. <paramref name="untilRestart"/>: a broken game patch, stays stopped until the game restarts.
    /// Otherwise (repeated errors) the next world loaded starts it again.
    /// </summary>
    public static void Disable(string reason, bool untilRestart = true)
    {
        failed = true;
        failedUntilRestart |= untilRestart;
        PluginLog.Error("Stopped: " + reason + (untilRestart ? " (until the game restarts)" : " (until another world is loaded)"));
    }

    /// <summary>
    /// Esc (the game's menu key) while the dock camera or a turret is active: leave that instead of opening the
    /// in-game menu. Called by EscapePatch before the game opens the menu; true = the menu must not open.
    /// Not while another screen is open (there Esc closes that screen) or after the plugin stopped itself.
    /// </summary>
    public static bool HandleEscape()
    {
        if (failed || UiState.IsCursorScreenOpen()) return false;
        try
        {
            if (dockActive)
            {
                Exit("Esc");
                return true;
            }
            if (turretBlock != null && turretControl != null)
            {
                ReleaseTurret();
                return true;
            }
        }
        catch (Exception e)
        {
            PluginLog.Warn("Esc handling failed, opening the menu instead: " + e.Message);
        }
        return false;
    }

    /// <summary>
    /// The client world is being unloaded (called by WorldUnloadPatch before the game disposes it): give the view
    /// back and drop every reference into that world, so it is not kept in memory while in the main menu.
    /// </summary>
    public static void OnWorldUnloading()
    {
        if (clientSession == null) return;
        try { ExitAll("world unloaded"); } catch { /* the world is going away anyway */ }
        LastDockIndexPerGrid.Clear();
        CameraNotice.Forget();
        clientSession = null;
        PluginLog.Info("World unloaded: camera state cleared.");
    }

    public static void OnUpdate(Session session)
    {
        if (failedUntilRestart) return;

        // Client session only (the in-process server session has no in-game UI).
        if (session.TryGet<SessionInGameUISessionComponent>() == null) return;

        // A new world: drop everything that points into the old one, and start again after an error stop.
        if (clientSession == null || !clientSession.TryGetTarget(out var known) || !ReferenceEquals(known, session))
        {
            try { ExitAll("world changed"); } catch { /* old world is gone */ }
            LastDockIndexPerGrid.Clear();
            CameraNotice.Forget();
            clientSession = new WeakReference<Session>(session);
            if (failed)
            {
                failed = false;
                errorCount = 0;
                PluginLog.Info("New world: started again after the error stop.");
            }
        }

        if (failed) return;

        try
        {
            if (!Config.Current.Enabled)
            {
                ExitAll("plugin disabled");
                return;
            }

            Input.Read(session);
            var state = ControlState.Read(session);
            if (state.Controller == null)
            {
                ExitAll("no local player");
                return;
            }

            if (dockActive)
                UpdateActive(session, state);
            else if (Input.Toggle && state.Mode == ControlMode.Cockpit)
                Enter(session, state);

            UpdateTurretCamera(state);
        }
        catch (Exception e)
        {
            // Count only errors that come in a row; an old one-off error must not add up to a stop later.
            if (Stopwatch.GetElapsedTime(lastErrorTicks).TotalSeconds > 10) errorCount = 0;
            lastErrorTicks = Stopwatch.GetTimestamp();
            errorCount++;
            PluginLog.Error($"Update failed ({errorCount}): {e}");
            try { ExitAll("error"); } catch { /* ignore */ }
            if (errorCount >= 5) Disable("too many errors, see log", untilRestart: false);
        }
    }

    #region Camera view

    private static void Enter(Session session, ControlState state)
    {
        if (state.Camera == null || state.Grid == null)
        {
            PluginLog.WarnOnce("enter-nocam", $"Cannot enter camera view: camera={state.Camera != null}, grid={state.Grid != null}");
            return;
        }

        dockPoints = CameraPoints.Find(session, state.Grid);
        if (dockPoints.Count == 0)
        {
            PluginLog.Info(Config.Current.AllConnectors ? "No connectors on this grid." : "No connector with [CAM] in its name on this grid.");
            return;
        }

        dockGridEntity = state.Grid.Entity;
        dockIndex = LastDockIndexPerGrid.TryGetValue(dockGridEntity, out var last) && last < dockPoints.Count ? last : 0;
        dockRenderCamera = state.Camera;
        dockFovDegrees = Config.Current.WideViewFov;
        dockActive = true;

        dockRenderCamera.SetNextTransitionNonSmooth();
        ApplyFov();
        CameraNotice.Show(session, dockPoints[dockIndex].Name, dockIndex + 1, dockPoints.Count);
        PluginLog.Info($"Camera view on: {dockPoints[dockIndex].Name} ({dockIndex + 1}/{dockPoints.Count})");
        LogPoints();
    }

    private static void UpdateActive(Session session, ControlState state)
    {
        // Leave camera view when the player leaves the cockpit, switches grid or presses the key again.
        var stillHere = state.Mode == ControlMode.Cockpit && state.Grid?.Entity == dockGridEntity;
        if (Input.Toggle || !stillHere || state.Camera == null)
        {
            Exit(Input.Toggle ? "key" : "left the cockpit");
            return;
        }

        // The render camera can be replaced (e.g. after a camera mode change): move the override over.
        if (!ReferenceEquals(state.Camera, dockRenderCamera))
        {
            ClearOverride(dockRenderCamera);
            dockRenderCamera = state.Camera;
            ApplyFov();
        }

        if (Input.Next || Input.Previous)
            Switch(session, Input.Next ? 1 : -1);

        // Drop destroyed or removed camera blocks.
        if (!dockPoints[dockIndex].TryGetPose(state.ControlledBlock, out var pose))
        {
            dockPoints.RemoveAt(dockIndex);
            if (dockPoints.Count == 0)
            {
                Exit("camera destroyed");
                return;
            }
            dockIndex = Cycle.Next(dockIndex - 1, dockPoints.Count, 1);
            dockRenderCamera.SetNextTransitionNonSmooth();
            CameraNotice.Show(session, dockPoints[dockIndex].Name, dockIndex + 1, dockPoints.Count);
            PluginLog.Info($"Camera lost, now: {dockPoints[dockIndex].Name} ({dockIndex + 1}/{dockPoints.Count})");
            return; // the new camera is shown from the next frame
        }

        if (Config.Current.WheelZoom && Input.WheelNotches != 0)
        {
            dockFovDegrees = Zoom.Step(dockFovDegrees, Input.WheelNotches, Zoom.MaxZoomDegrees, Config.Current.WideViewFov);
            ApplyFov();
        }

        dockRenderCamera.SetTransformOverride(pose);

        if (Config.Current.ControlsFollowCamera)
            CameraControls.Set(state.Grid, dockPoints[dockIndex].LastForward, dockPoints[dockIndex].LastScreenUp);
        else
            CameraControls.Clear();
    }

    private static void Switch(Session session, int delta)
    {
        dockIndex = Cycle.Next(dockIndex, dockPoints.Count, delta);
        LastDockIndexPerGrid[dockGridEntity] = dockIndex;
        dockRenderCamera.SetNextTransitionNonSmooth();
        CameraNotice.Show(session, dockPoints[dockIndex].Name, dockIndex + 1, dockPoints.Count);
        PluginLog.Info($"Camera: {dockPoints[dockIndex].Name} ({dockIndex + 1}/{dockPoints.Count})");
    }

    private static void Exit(string reason)
    {
        if (!dockActive) return;
        dockActive = false;
        CameraControls.Clear();
        if (dockGridEntity != null && dockIndex >= 0) LastDockIndexPerGrid[dockGridEntity] = dockIndex;

        ClearOverride(dockRenderCamera);

        dockRenderCamera = null;
        dockGridEntity = null;
        dockPoints = new List<CameraPoint>();
        PluginLog.Info("Camera view off: " + reason);
    }

    private static void ApplyFov()
    {
        var normal = Config.Current.WideViewFov;
        dockRenderCamera.SetCustomFOV(Zoom.ToRadians(dockFovDegrees), Zoom.SensitivityFor(dockFovDegrees, normal));
    }

    /// <summary>Gives the view back to the game. Safe on a camera from an unloaded session.</summary>
    private static void ClearOverride(CameraComponent camera)
    {
        if (camera == null) return;
        try
        {
            camera.SetTransformOverride(null);
            camera.ResetCustomFOV();
            camera.SetNextTransitionNonSmooth();
        }
        catch (Exception e)
        {
            PluginLog.Verbose("Clear override on old camera failed (session unloaded?): " + e.Message);
        }
    }

    private static void LogPoints()
    {
        if (!Config.Current.VerboseLog) return;
        for (var i = 0; i < dockPoints.Count; i++)
        {
            var p = dockPoints[i];
            PluginLog.Info($"  #{i + 1} {p.Name}");
        }
    }

    #endregion

    #region Turret camera

    /// <summary>
    /// While the player controls a turret, the view is on the turret's barrel. The game does the aiming:
    /// mouse → AutomatedWeaponInputHandlerComponent → server, which keeps the turn speed and elevation limits.
    /// The plugin only moves the view with the barrel, zooms with the wheel and slows the aim while zoomed.
    /// </summary>
    private static void UpdateTurretCamera(ControlState state)
    {
        var config = Config.Current;
        var wanted = state.Mode == ControlMode.Turret && state.Camera != null && !dockActive && state.ControlledBlock != null;

        if (!wanted)
        {
            if (turretBlock != null) ExitTurret("turret control ended");
            return;
        }

        // A different turret, the render camera was replaced or the "Barrel view" setting changed: start over.
        if (turretBlock != null && (!ReferenceEquals(state.ControlledBlock, turretBlock)
                                    || !ReferenceEquals(state.Camera, turretRenderCamera)
                                    || turretBarrelViewSetting != config.BarrelView))
            ExitTurret("turret changed");

        if (turretBlock == null) EnterTurret(state);

        // In turret control Esc did not reach the game's menu hotkey in testing (no EscapePatch call), so read it here too.
        if (Input.Escape && turretControl != null)
        {
            ReleaseTurret();
            return;
        }

        // Right mouse button reloads, as with manually controlled weapons. The game's own reload key keeps working.
        if (Input.RightClick && turretControl != null)
            turretControl.RequestReload();

        // Zoom works in the barrel view and in the game's own turret view.
        if (Input.WheelNotches != 0 && config.WheelZoom)
        {
            turretFovDegrees = Zoom.Step(turretFovDegrees, Input.WheelNotches, Zoom.MaxZoomDegrees, turretNormalFovDegrees);
            ApplyTurretFov();
        }

        if (barrelView == null) return; // game's own view, zoom only

        if (barrelView.TryGetPose(out var pose))
            turretRenderCamera.SetTransformOverride(pose);
        else
            ExitTurret("turret destroyed");
    }

    /// <summary>
    /// Starts turret control: always the wheel zoom; the barrel view when "Barrel view" is on and the turret qualifies
    /// ("All turrets" on, or [CAM] in its name).
    /// </summary>
    private static void EnterTurret(ControlState state)
    {
        var config = Config.Current;
        turretBlock = state.ControlledBlock;
        turretControl = state.TurretControl;
        turretReleaseRequested = false;
        turretRenderCamera = state.Camera;
        turretBarrelViewSetting = config.BarrelView;
        turretNormalFovDegrees = Math.Max(Zoom.ToDegrees(state.Camera.FieldOfView), Zoom.MaxZoomDegrees);
        turretFovDegrees = turretNormalFovDegrees;
        TurretAimSensitivity = 1f;
        barrelView = null;

        var name = BlockName(state.ControlledBlock);
        var tagged = CameraTags.TryParse(name, out var tag);
        if (!config.BarrelView || (!config.AllTurrets && !tagged))
        {
            PluginLog.Info($"Turret control: {name} (game view, wheel zoom)");
            return;
        }

        var camera = TurretCamera.TryCreate(state.ControlledBlock, state.TurretPart, tagged && tag.Label.Length > 0 ? tag.Label : name);
        if (camera == null)
        {
            PluginLog.WarnOnce("turret-none", "Turret control found, but no turret parts: game view with wheel zoom.");
            return;
        }

        barrelView = camera;
        turretRenderCamera.SetNextTransitionNonSmooth();
        PluginLog.Info($"Turret camera on: {barrelView.Name} (game camera point: {(barrelView.HasGameCameraPoint ? "yes" : "no")}, " +
                       $"definition: {barrelView.DefinitionName}, view moved right/up: {barrelView.ViewShift.X} m / {barrelView.ViewShift.Y} m)");
    }

    /// <summary>Esc during turret control: the same request as the game's release key (F), once per turret.</summary>
    private static void ReleaseTurret()
    {
        if (turretReleaseRequested) return;
        turretReleaseRequested = true;
        turretControl.RequestReleaseControl();
        PluginLog.Info("Esc: turret control released");
    }

    private static void ApplyTurretFov()
    {
        var sensitivity = Zoom.SensitivityFor(turretFovDegrees, turretNormalFovDegrees);
        turretRenderCamera.SetCustomFOV(Zoom.ToRadians(turretFovDegrees), sensitivity);
        TurretAimSensitivity = sensitivity;
    }

    private static void ExitTurret(string reason)
    {
        if (turretBlock == null) return;
        ClearOverride(turretRenderCamera); // also resets the zoom
        var hadBarrelView = barrelView != null;
        barrelView = null;
        turretBlock = null;
        turretControl = null;
        turretRenderCamera = null;
        TurretAimSensitivity = 1f;
        PluginLog.Info((hadBarrelView ? "Turret camera off: " : "Turret control off: ") + reason);
    }

    private static string BlockName(Entity block)
    {
        try { return block?.TryGet<FunctionalBlockComponent>()?.DisplayName.EvaluateText() ?? ""; }
        catch { return ""; }
    }

    #endregion

    private static void ExitAll(string reason)
    {
        Exit(reason);
        ExitTurret(reason);
    }
}
