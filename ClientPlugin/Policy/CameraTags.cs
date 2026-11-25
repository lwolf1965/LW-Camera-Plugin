using System;

namespace ClientPlugin.Policy;

/// <summary>A parsed [CAM ...] name tag.</summary>
public sealed class CameraTag
{
    /// <summary>Block name without the tag, trimmed. Shown as the camera name.</summary>
    public string Label = "";
}

/// <summary>
/// Parses camera name tags. Pure logic, no game types (compiled into tests/).
///
/// Format: [CAM], case-insensitive, anywhere in the block name. Anything else in the bracket ([CAMERA], [CAM 2],
/// [Cam stuff]) is not our tag.
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
        tag = inner.Equals(Keyword, StringComparison.OrdinalIgnoreCase) ? new CameraTag() : null;
        return tag != null;
    }
}
