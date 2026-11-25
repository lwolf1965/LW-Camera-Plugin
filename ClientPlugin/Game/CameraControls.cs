using Keen.Game2.Simulation.WorldObjects.CubeGrids;
using Keen.VRage.Core;
using Keen.VRage.Core.Game.Components;
using Keen.VRage.Core.Game.Data;
using Keen.VRage.DCS.Accessors;
using Keen.VRage.DCS.Components;
using Keen.VRage.Library.Mathematics;

namespace ClientPlugin.Game;

/// <summary>
/// "Controls follow dock camera": while the dock camera is on, W A S D C Space and the mouse act in the camera's
/// axes instead of the cockpit's. W flies where the camera looks, Space moves up on the screen, the mouse turns the
/// ship around the camera's axes.
///
/// Game API: the cockpit sends its inputs with EntityMovementExtensions.UpdateControlData(gridData, inputs,
/// rotation, isAngular). "rotation" is the cockpit block's orientation in the grid (its RelativeTransform), and the
/// game turns the movement and rotation inputs by it. ControlsFollowCameraPatch swaps it for the camera's
/// orientation in the grid, computed here.
/// </summary>
internal static class CameraControls
{
    private static bool active;
    private static DEntityContext gridData;
    private static Quaternion cameraInGrid;

    /// <summary>Called every frame while the dock camera shows a connector.</summary>
    public static void Set(CubeGridComponent grid, Vector3D cameraForward, Vector3D cameraUp)
    {
        WorldTransform gridWorld = grid.Entity.Data.GetWorldTransform();
        cameraInGrid = ToGrid(gridWorld.Orientation, cameraForward, cameraUp);
        gridData = grid.Entity.Data;
        active = true;
    }

    public static void Clear()
    {
        active = false;
        gridData = default;
    }

    /// <summary>From the patch: replaces the cockpit's orientation when the inputs go to the dock camera's grid.</summary>
    public static void Apply(in DEntityContext data, ref Quaternion? rotation)
    {
        if (!active || !rotation.HasValue || !data.Equals(gridData)) return;
        rotation = cameraInGrid;
    }

    /// <summary>Orientation in the grid's frame of a view with these world axes.</summary>
    internal static Quaternion ToGrid(Quaternion gridWorld, Vector3D forward, Vector3D up)
    {
        var toGrid = Quaternion.Inverse(gridWorld);
        Vector3D localForward = toGrid * (Vector3)forward;
        Vector3D localUp = toGrid * (Vector3)up;
        MatrixD m = MatrixD.CreateWorld(Vector3D.Zero, localForward, localUp);
        return Quaternion.CreateFromRotationMatrix(in m);
    }
}
