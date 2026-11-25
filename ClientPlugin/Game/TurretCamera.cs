using System;
using System.Collections.Generic;
using Keen.Game2.Client.WorldObjects.CubeBlocks.Weapons;
using Keen.Game2.Simulation.WorldObjects.CubeBlocks.Weapons;
using Keen.VRage.Core;
using Keen.VRage.Core.Game.Components;
using Keen.VRage.Core.Game.Data;
using Keen.VRage.DCS.Components;
using Keen.VRage.Game.Components.Models;
using Keen.VRage.Library.Definitions;
using Keen.VRage.Library.Mathematics;
using Keen.VRage.Library.Utils;

namespace ClientPlugin.Game;

/// <summary>
/// The barrel view of one turret: the camera point the game itself defines for direct turret control.
///
/// Game API:
///  - DirectControlConvergeTargetingDefinition (Game2.Simulation): CameraModel (the barrel part's model),
///    CameraDummyType (a dummy in that model), CameraAdditionalOffset (block space). The server's
///    DirectControlConvergeTargetingComponent makes shots converge on what this point looks at:
///    position = block * (turretOrientation * dummyPosition + additionalOffset), looking along the barrel.
///    The component is server-only, but definitions are loaded on the client too:
///    Singleton&lt;DefinitionManager&gt;.Instance.GetDefinitionsOfType&lt;T&gt;().
///  - The barrel part is the deepest descendant with AzimuthElevationRotationComponent (the client turns these
///    parts on AutoTargetingComponent.SyncAzimuthElevationSignal). Its ModelComponent.Definition is the CameraModel.
///  - Angle limits: the server clamps elevation (AutoTargetingDefinition.Min/MaxElevation) and turn speed;
///    azimuth is a full 360°. The view follows the barrel, so it can never look past the limits.
/// </summary>
internal sealed class TurretCamera
{
    private readonly struct CameraSpec
    {
        public readonly Vector3 DummyPosition;
        public readonly Vector3 AdditionalOffset;
        public readonly Vector3 ViewShift;
        public readonly string DebugName;

        public CameraSpec(Vector3 dummyPosition, Vector3 additionalOffset, Vector3 viewShift, string debugName)
        {
            DummyPosition = dummyPosition;
            AdditionalOffset = additionalOffset;
            ViewShift = viewShift;
            DebugName = debugName;
        }
    }

    /// <summary>
    /// Plugin's own correction of the game's camera point, in barrel axes (X right, Y up, metres).
    /// The Gatling turret's point sits beside the barrel, which then fills the lower left of the view.
    /// Matched by the definition's debug name; turrets not listed (Rocket turret) keep the game's point.
    /// </summary>
    private static readonly (string NamePart, Vector3 Shift)[] ViewShifts =
    {
        ("gatling", new Vector3(-0.3f, 0.3f, 0f)),
    };

    // Per barrel model; null = this model has no camera definition (fallback view).
    private static readonly Dictionary<ModelComponentDefinition, CameraSpec?> SpecCache = new();

    private readonly Entity block;
    private readonly Entity barrel;

    /// <summary>The plugin's shift of the game's camera point, in barrel axes (X right, Y up, metres).</summary>
    public Vector3 ViewShift => spec?.ViewShift ?? Vector3.Zero;

    /// <summary>Debug name of the game's camera definition (for the log), empty without one.</summary>
    public string DefinitionName => spec?.DebugName ?? "";
    private readonly CameraSpec? spec;

    public string Name { get; }

    /// <summary>True when the game's own camera point was found (the crosshair is where shots converge).</summary>
    public bool HasGameCameraPoint => spec.HasValue;

    private TurretCamera(Entity block, Entity barrel, CameraSpec? spec, string name)
    {
        this.block = block;
        this.barrel = barrel;
        this.spec = spec;
        Name = name;
    }

    public bool IsAlive
    {
        get
        {
            try { return block?.Scene != null && barrel?.Scene != null; }
            catch { return false; }
        }
    }

    public static TurretCamera TryCreate(Entity block, Entity controllable, string name)
    {
        if (block == null) return null;
        var barrel = FindBarrel(block) ?? controllable;
        if (barrel == null) return null;

        var model = controllable?.TryGet<ModelComponent>()?.Definition ?? barrel.TryGet<ModelComponent>()?.Definition;
        var spec = model != null ? GetSpec(model) : null;
        if (!spec.HasValue)
            PluginLog.WarnOnce("turret-spec:" + name, $"No game camera point for turret '{name}': using the barrel pivot.");
        return new TurretCamera(block, barrel, spec, name);
    }

    /// <summary>World pose of the barrel view, or false when the turret is gone.</summary>
    public bool TryGetPose(out WorldTransform pose)
    {
        pose = WorldTransform.Identity;
        if (!IsAlive) return false;

        try
        {
            MatrixD blockWorld = block.Data.GetWorldTransform();
            MatrixD barrelWorld = barrel.Data.GetWorldTransform();
            Vector3D forward = barrelWorld.Forward;
            Vector3D up = barrelWorld.Up;

            Vector3D position;
            if (spec.HasValue)
            {
                // Same point as the server's converge targeting: the dummy turns with the turret, the extra offset does not.
                var turretRotation = barrelWorld * MatrixD.Invert(in blockWorld);
                var local = Vector3D.TransformNormal(spec.Value.DummyPosition, in turretRotation) + (Vector3D)spec.Value.AdditionalOffset;
                position = Vector3D.Transform(in local, in blockWorld);
                var shift = spec.Value.ViewShift;
                position += barrelWorld.Right * shift.X + up * shift.Y;
            }
            else
            {
                position = barrelWorld.Translation + forward * CameraPoint.FaceOffset;
            }

            var rotation = MatrixD.CreateWorld(in position, in forward, in up);
            pose = new WorldTransform(position, Quaternion.CreateFromRotationMatrix(in rotation));
            return true;
        }
        catch (Exception e)
        {
            PluginLog.Verbose("Turret pose failed: " + e.Message);
            return false;
        }
    }

    private static Entity FindBarrel(Entity block)
    {
        Entity last = null;
        foreach (var part in block.GetDescendants(includeRoot: false, recursive: true, depthFirst: false,
                     e => e.TryGet<AzimuthElevationRotationComponent>() != null))
            last = part;
        return last;
    }

    private static Vector3 ViewShiftFor(string debugName)
    {
        foreach (var (namePart, shift) in ViewShifts)
            if (debugName.Contains(namePart, StringComparison.OrdinalIgnoreCase))
                return shift;
        return Vector3.Zero;
    }

    private static CameraSpec? GetSpec(ModelComponentDefinition model)
    {
        if (SpecCache.TryGetValue(model, out var cached)) return cached;

        CameraSpec? found = null;
        foreach (var definition in Singleton<DefinitionManager>.Instance.GetDefinitionsOfType<DirectControlConvergeTargetingDefinition>())
        {
            if (!ReferenceEquals(definition.CameraModel, model) || definition.CameraDummyType == null) continue;
            var dummy = definition.CameraModel.TryGetDummy(definition.CameraDummyType);
            if (dummy == null) continue;
            var debugName = definition.DebugName + " / " + model.DebugName;
            found = new CameraSpec(dummy.Transform.Transform.Position, definition.CameraAdditionalOffset, ViewShiftFor(debugName), debugName);
            break;
        }

        SpecCache[model] = found;
        return found;
    }
}
