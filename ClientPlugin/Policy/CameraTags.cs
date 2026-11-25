using System;
using System.Globalization;

namespace ClientPlugin.Policy;

/// <summary>A parsed [CAM ...] name tag.</summary>
public sealed class CameraTag
{
    /// <summary>Block name without the tag, trimmed. Shown as the camera name.</summary>
    public string Label = "";

    /// <summary>Metres in front of the connector's face. Null = use the default.</summary>
    public float? Offset;
}

/// <summary>
/// Parses camera name tags. Pure logic, no game types (compiled into tests/).
///
/// Formats (case-insensitive, anywhere in the block name):
///   [CAM]                  default offset
///   [CAM 2]                camera 2 m in front of the connector's face
/// </summary>
public static class CameraTags
{
    public const string Keyword = "CAM";

    public static bool TryParse(string blockName, out CameraTag tag)
    {
        tag = null;
        if (string.IsNullOrEmpty(blockName)) return false;

        var search = 0;
        while (true)
        {
            var open = blockName.IndexOf('[', search);
            if (open < 0) return false;
            var close = blockName.IndexOf(']', open + 1);
            if (close < 0) return false;

            var inner = blockName.Substring(open + 1, close - open - 1).Trim();
            if (TryParseInner(inner, out tag))
            {
                tag.Label = (blockName.Substring(0, open) + blockName.Substring(close + 1)).Trim();
                while (tag.Label.Contains("  ")) tag.Label = tag.Label.Replace("  ", " ");
                return true;
            }
            search = close + 1;
        }
    }

    private static bool TryParseInner(string inner, out CameraTag tag)
    {
        tag = null;
        var parts = inner.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || !parts[0].Equals(Keyword, StringComparison.OrdinalIgnoreCase)) return false;

        var result = new CameraTag();
        for (var i = 1; i < parts.Length; i++)
        {
            var part = parts[i];
            if (float.TryParse(part.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var offset))
                result.Offset = Math.Clamp(offset, -50f, 50f);
            else
                return false; // unknown word: not our tag (e.g. [CAMERA] or [Cam stuff])
        }
        tag = result;
        return true;
    }
}
