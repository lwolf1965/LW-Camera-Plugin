using System;
using System.Collections.Generic;
using System.Linq;
using ClientPlugin.Policy;
using Keen.Game2.Simulation.WorldObjects.CubeBlocks.MechanicalBlocks;
using Keen.Game2.Simulation.WorldObjects.CubeBlocks;
using Keen.Game2.Simulation.WorldObjects.CubeGrids;
using Keen.VRage.Core;
using Keen.VRage.Core.Game.Components;
using Keen.VRage.Core.Game.Data;
using Keen.VRage.Core.Game.Systems;
using Keen.VRage.DCS.Components;
using Keen.VRage.Library.Mathematics;

namespace ClientPlugin.Game;

/// <summary>A dock camera: a fixed view out of a connector.</summary>
internal sealed class CameraPoint
{
    public Entity Block;
    public ConnectorComponent Connector;

    public string Name;
    public float Offset;

    /// <summary>World direction of the top of the screen from the last pose.</summary>
    public Vector3D LastScreenUp;

    /// <summary>World direction the camera looked in the last pose.</summary>
    public Vector3D LastForward;

    /// <summary>False once the connector is removed from the world. Never throws.</summary>
    public bool IsAlive
    {
        get
        {
            try { return Block?.Scene != null; }
            catch { return false; }
        }
    }

    /// <summary>
    /// World pose of the camera: the connector's pivot (the centre of its connecting face), moved Offset metres
    /// out of the connector and looking straight out of it.
    /// The top of the screen follows the cockpit, not the connector: a connector looks the same in every rotation
    /// around its axis, so its own "up" would roll or flip the image and the controls would not match the screen.
    /// Game API: MechanicalBlockComponent.WorldSpacePivotTransform. The pivot's Forward points out of the
    /// connector: AlignmentLimits.AreWithinLimits requires two connectors' pivot Forwards to face each other.
    /// </summary>
    /// <param name="cockpit">The cockpit block the player sits in; null keeps the connector's own up.</param>
    public bool TryGetPose(Entity cockpit, out WorldTransform pose)
    {
        pose = WorldTransform.Identity;
        if (!IsAlive) return false;

        try
        {
            MatrixD m = Connector.WorldSpacePivotTransform;

            Vector3D forward = m.Forward;
            Vector3D up = m.Up;
            if (cockpit != null)
            {
                MatrixD seat = cockpit.Data.GetWorldTransform();
                up = ScreenUp(forward, seat, up);
            }
            LastScreenUp = up;
            LastForward = forward;
            var position = m.Translation + forward * Offset;
            var rotation = MatrixD.CreateWorld(position, forward, up);
            pose = new WorldTransform(position, Quaternion.CreateFromRotationMatrix(in rotation));
            return true;
        }
        catch (Exception e)
        {
            // The connector was removed between the alive check and the read.
            PluginLog.Verbose("Camera pose failed: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Screen up for a view along <paramref name="forward"/>: the cockpit's up, or for a view straight up or down
    /// (along the cockpit's up axis) the cockpit's tail or nose, like tilting your head in the seat.
    /// </summary>
    internal static Vector3D ScreenUp(Vector3D forward, in MatrixD cockpit, Vector3D fallback)
    {
        if (TryPerpendicular(cockpit.Up, forward, out var up)) return up;
        var lookingDown = Vector3D.Dot(in forward, cockpit.Down) > 0;
        if (TryPerpendicular(lookingDown ? cockpit.Forward : cockpit.Backward, forward, out up)) return up;
        return fallback;
    }

    /// <summary>The part of <paramref name="reference"/> perpendicular to <paramref name="forward"/>, normalized.</summary>
    private static bool TryPerpendicular(Vector3D reference, Vector3D forward, out Vector3D result)
    {
        var n = Vector3D.Normalize(forward);
        var v = reference - n * Vector3D.Dot(in reference, in n);
        var lengthSquared = Vector3D.Dot(in v, in v);
        // Below ~25° between the reference and the view axis the result gets unstable: use the next reference.
        if (lengthSquared < 0.18)
        {
            result = default;
            return false;
        }
        result = v * (1.0 / Math.Sqrt(lengthSquared));
        return true;
    }
}

/// <summary>
/// Finds the dock cameras of a grid: its connectors. Other blocks are not cameras, tagged or not.
/// A [CAM ...] tag on a connector is optional: it sets the name and offset, and with
/// "All connectors are cameras" off it is what makes the connector a camera.
///
/// Game API:
///  - Session.GetEntitiesOfType&lt;FunctionalBlockComponent&gt;() — all functional blocks (as in LW Cargo Manager)
///  - FunctionalBlockComponent.Grid, .DisplayName.EvaluateText()
///  - ConnectorComponent (Game2.Simulation, MechanicalBlocks) on the connector block
/// </summary>
internal static class CameraPoints
{
    public static List<CameraPoint> Find(Session session, CubeGridComponent grid)
    {
        var result = new List<CameraPoint>();
        if (grid == null) return result;
        var config = Config.Current;

        foreach (var entity in session.GetEntitiesOfType<FunctionalBlockComponent>())
        {
            var block = entity.TryGet<FunctionalBlockComponent>();
            if (block == null || block.Grid != grid) continue;

            var connector = FindConnector(entity);
            if (connector == null) continue;

            string name;
            try { name = block.DisplayName.EvaluateText() ?? ""; }
            catch { name = ""; }

            var tagged = CameraTags.TryParse(name, out var tag);
            if (!tagged && !config.AllConnectors) continue;

            result.Add(new CameraPoint
            {
                Block = entity,
                Connector = connector,
                Name = tagged && tag.Label.Length > 0 ? tag.Label : (name.Length > 0 ? name : "Connector"),
                Offset = tagged && tag.Offset.HasValue ? tag.Offset.Value : config.CameraOffset,
            });
        }

        // Alphabetical: a stable order for cycling.
        return result.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static ConnectorComponent FindConnector(Entity block)
    {
        try
        {
            var own = block.TryGet<ConnectorComponent>();
            if (own != null) return own;
            foreach (var part in block.GetDescendants(includeRoot: false, recursive: true, depthFirst: false,
                         e => e.TryGet<ConnectorComponent>() != null))
                return part.TryGet<ConnectorComponent>();
        }
        catch (Exception e)
        {
            PluginLog.WarnOnce("connector", "Connector search failed: " + e.Message);
        }
        return null;
    }
}
