using System;

namespace ClientPlugin.Policy;

/// <summary>Field-of-view zoom math. Pure logic, no game types (compiled into tests/).</summary>
public static class Zoom
{
    /// <summary>
    /// One mouse-wheel notch changes the FOV by this factor. 8 notches go from 60° to about 15°,
    /// which feels close to the SE1 camera block.
    /// </summary>
    public const float DefaultStepFactor = 1.19f;

    /// <summary>Smallest field of view (strongest zoom), about the SE1 camera block's.</summary>
    public const float MaxZoomDegrees = 6f;

    /// <summary>
    /// New FOV (degrees) after <paramref name="notches"/> wheel notches. Positive = zoom in (smaller FOV).
    /// Clamped to [min, max].
    /// </summary>
    public static float Step(float fovDegrees, int notches, float minDegrees, float maxDegrees, float stepFactor = DefaultStepFactor)
    {
        if (minDegrees > maxDegrees) (minDegrees, maxDegrees) = (maxDegrees, minDegrees);
        if (stepFactor <= 1f) stepFactor = DefaultStepFactor;
        var fov = fovDegrees * MathF.Pow(stepFactor, -notches);
        return Math.Clamp(fov, minDegrees, maxDegrees);
    }

    /// <summary>
    /// Mouse sensitivity multiplier for a zoomed view, the same formula the game uses for
    /// aim-down-sights: tan(zoomed/2) / tan(normal/2). 1 when not zoomed.
    /// </summary>
    public static float SensitivityFor(float zoomedDegrees, float normalDegrees)
    {
        if (zoomedDegrees <= 0f || normalDegrees <= 0f) return 1f;
        var zoomed = MathF.Tan(ToRadians(zoomedDegrees) / 2f);
        var normal = MathF.Tan(ToRadians(normalDegrees) / 2f);
        return normal <= 0f ? 1f : Math.Clamp(zoomed / normal, 0.01f, 1f);
    }

    /// <summary>Turns a raw wheel value into whole notches. Values are accumulated so small deltas are not lost.</summary>
    public static int TakeNotches(ref float accumulator, float raw, float unitsPerNotch)
    {
        if (unitsPerNotch <= 0f) unitsPerNotch = 1f;
        accumulator += raw / unitsPerNotch;
        var notches = (int)accumulator; // truncates toward zero
        accumulator -= notches;
        return notches;
    }

    public static float ToRadians(float degrees) => degrees * MathF.PI / 180f;

    public static float ToDegrees(float radians) => radians * 180f / MathF.PI;
}

/// <summary>Cycling through a camera list.</summary>
public static class Cycle
{
    /// <summary>Wraps around in both directions. Returns -1 if the list is empty.</summary>
    public static int Next(int index, int count, int delta)
    {
        if (count <= 0) return -1;
        var next = (index + delta) % count;
        return next < 0 ? next + count : next;
    }
}
