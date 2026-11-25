using Keen.Game2.Client.GameSystems.CameraSystems;
using Keen.Game2.Client.GameSystems.PlayerControl;
using Keen.Game2.Client.WorldObjects.CubeBlocks.Cockpit;
using Keen.Game2.Client.WorldObjects.CubeBlocks.Weapons;
using Keen.Game2.Simulation.WorldObjects.CubeBlocks;
using Keen.Game2.Simulation.WorldObjects.CubeGrids;
using Keen.VRage.Core.Game.Components;
using Keen.VRage.Core.Game.Systems;
using Keen.VRage.DCS.Components;

namespace ClientPlugin.Game;

internal enum ControlMode
{
    None,
    Cockpit,
    Turret,
}

/// <summary>
/// What the local player controls right now, and the render camera.
///
/// Game API (Game2.Client):
///  - ClientPlayersSessionComponent.LocalPlayerController (PlayerControllerComponent : IController)
///  - IController.GetTopControllable() → the cockpit or the turret the player controls
///  - IController.CameraSystem.RenderCameraEntity.Get&lt;CameraComponent&gt;() → the render camera
///    (same path as GunToolCameraADSComponent, the aim-down-sights zoom)
///  - CockpitControllableComponent on the cockpit, AutomatedWeaponBlockToolControllableComponent on a turret
///    (its BlockEntity is the turret block)
/// </summary>
internal sealed class ControlState
{
    public ControlMode Mode;
    public IController Controller;
    public CameraComponent Camera;
    public CameraSystemComponent CameraSystem;

    /// <summary>The cockpit or turret block entity.</summary>
    public Entity ControlledBlock;

    public CubeGridComponent Grid;

    /// <summary>Turret only: the entity with AutomatedWeaponBlockToolControllableComponent (the barrel part).</summary>
    public Entity TurretPart;

    /// <summary>Turret only: the game's controllable; RequestReleaseControl() is what the release key calls.</summary>
    public AutomatedWeaponBlockToolControllableComponent TurretControl;

    public static ControlState Read(Session session)
    {
        var state = new ControlState();
        var controller = session.TryGet<ClientPlayersSessionComponent>()?.LocalPlayerController;
        if (controller == null) return state;
        state.Controller = controller;

        var cameraSystem = controller.CameraSystem;
        state.Camera = cameraSystem?.RenderCameraEntity?.TryGet<CameraComponent>();
        state.CameraSystem = cameraSystem as CameraSystemComponent;

        var top = controller.GetTopControllable();
        if (top == null) return state;

        if (top is AutomatedWeaponBlockToolControllableComponent turret)
        {
            state.Mode = ControlMode.Turret;
            state.ControlledBlock = FindBlockEntity(turret.BlockEntity) ?? turret.BlockEntity;
            state.TurretPart = turret.Entity;
            state.TurretControl = turret;
        }
        else if (FindUp<CockpitControllableComponent>(top.Entity) != null)
        {
            state.Mode = ControlMode.Cockpit;
            state.ControlledBlock = FindBlockEntity(top.Entity);
        }

        state.Grid = state.ControlledBlock?.TryGet<FunctionalBlockComponent>()?.Grid;
        if (state.Grid == null && state.Mode != ControlMode.None)
            PluginLog.WarnOnce("nogrid", $"Controlled {state.Mode} found, but no grid. Entity: {top.Entity}");
        return state;
    }

    /// <summary>The entity or its nearest parent with component T.</summary>
    public static T FindUp<T>(Entity entity) where T : Component
    {
        if (entity == null) return null;
        var own = entity.TryGet<T>();
        if (own != null) return own;
        foreach (var parent in entity.GetParentsSequence())
        {
            var found = parent.TryGet<T>();
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>The block entity (has FunctionalBlockComponent) that owns this entity.</summary>
    public static Entity FindBlockEntity(Entity entity) => FindUp<FunctionalBlockComponent>(entity)?.Entity;
}
