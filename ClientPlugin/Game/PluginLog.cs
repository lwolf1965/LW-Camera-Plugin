using System.Collections.Generic;
using Keen.VRage.Library.Diagnostics;

namespace ClientPlugin.Game;

/// <summary>All plugin log lines start with [LwCameraPlugin] so they are easy to find in the game log.</summary>
internal static class PluginLog
{
    private static readonly HashSet<string> Once = new();

    public static void Info(string message) => Log.Default.WriteLine($"[{Plugin.Name}] {message}");

    public static void Warn(string message) => Log.Default.WriteLine(LogSeverity.Warning, $"[{Plugin.Name}] {message}");

    public static void Error(string message) => Log.Default.WriteLine(LogSeverity.Error, $"[{Plugin.Name}] {message}");

    /// <summary>Logs a message only the first time its key is seen (avoids flooding the log every frame).</summary>
    public static void WarnOnce(string key, string message)
    {
        if (Once.Add(key)) Warn(message);
    }

    public static void Verbose(string message)
    {
        if (Config.Current.VerboseLog)
            Info(message);
    }
}
