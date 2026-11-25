using System;
using ClientPlugin.Policy;

namespace LwCameraPlugin.Tests;

public static class CameraTagTests
{
    public static void PlainTag()
    {
        Assert.True(CameraTags.TryParse("Connector [CAM]", out var tag));
        Assert.Equal<float?>(null, tag.Offset);
        Assert.Equal("Connector", tag.Label);
    }

    public static void CaseInsensitiveAnywhere()
    {
        Assert.True(CameraTags.TryParse("[cam] Docking port", out var tag));
        Assert.Equal("Docking port", tag.Label);
        Assert.True(CameraTags.TryParse("Front [Cam] left", out tag));
        Assert.Equal("Front left", tag.Label);
    }

    public static void Offset()
    {
        Assert.True(CameraTags.TryParse("Nose [CAM 2]", out var tag));
        Assert.Equal<float?>(2f, tag.Offset);

        Assert.True(CameraTags.TryParse("X [CAM 0,5]", out tag), "comma decimal");
        Assert.Equal<float?>(0.5f, tag.Offset);
    }

    public static void DirectionWordsAreNotTags()
    {
        // The direction words were removed: the view always looks straight out of the connector.
        Assert.False(CameraTags.TryParse("Dock [CAM Down]", out _));
        Assert.False(CameraTags.TryParse("Dock [CAM back 1.5]", out _));
    }

    public static void NotOurTags()
    {
        Assert.False(CameraTags.TryParse("Camera block", out _));
        Assert.False(CameraTags.TryParse("Cargo [CAMERA]", out _));
        Assert.False(CameraTags.TryParse("Cargo [Cam stuff]", out _));
        Assert.False(CameraTags.TryParse("[Ore] container", out _));
        Assert.False(CameraTags.TryParse("", out _));
        Assert.False(CameraTags.TryParse(null, out _));
        Assert.False(CameraTags.TryParse("broken [CAM", out _));
    }

    public static void SkipsOtherTagsFirst()
    {
        // Cargo Manager tags on the same block must not hide the camera tag.
        Assert.True(CameraTags.TryParse("Connector [Unload] [CAM 1]", out var tag));
        Assert.Equal<float?>(1f, tag.Offset);
        Assert.Equal("Connector [Unload]", tag.Label);
    }

    public static void OffsetIsClamped()
    {
        Assert.True(CameraTags.TryParse("x [CAM 999]", out var tag));
        Assert.Equal<float?>(50f, tag.Offset);
    }
}

public static class ZoomTests
{
    public static void StepInAndOut()
    {
        var fov = Zoom.Step(60f, 1, 6f, 60f);
        Assert.True(fov < 60f && fov > 45f, $"one notch in: {fov}");
        Assert.Equal(60f, Zoom.Step(fov, -1, 6f, 60f), "one notch back out");
    }

    public static void Clamps()
    {
        Assert.Equal(6f, Zoom.Step(60f, 100, 6f, 60f), "min");
        Assert.Equal(60f, Zoom.Step(30f, -100, 6f, 60f), "max");
        Assert.Equal(6f, Zoom.Step(60f, 100, 60f, 6f), "swapped limits");
    }

    public static void SensitivityScalesWithZoom()
    {
        Assert.Equal(1f, Zoom.SensitivityFor(60f, 60f), "no zoom");
        var s = Zoom.SensitivityFor(10f, 60f);
        Assert.True(s > 0.1f && s < 0.2f, $"10 deg of 60: {s}");
    }

    public static void NotchAccumulator()
    {
        var acc = 0f;
        Assert.Equal(0, Zoom.TakeNotches(ref acc, 60f, 120f), "half notch");
        Assert.Equal(1, Zoom.TakeNotches(ref acc, 60f, 120f), "second half");
        Assert.Equal(-2, Zoom.TakeNotches(ref acc, -240f, 120f), "two back");
        Assert.Equal(3, Zoom.TakeNotches(ref acc, 3f, 1f), "unit notches");
    }
}

public static class CycleTests
{
    public static void Wraps()
    {
        Assert.Equal(1, Cycle.Next(0, 3, 1));
        Assert.Equal(0, Cycle.Next(2, 3, 1));
        Assert.Equal(2, Cycle.Next(0, 3, -1));
        Assert.Equal(-1, Cycle.Next(0, 0, 1));
    }
}

public static class TranslationTests
{
    public static void EveryRowHasAllLanguages()
    {
        foreach (var (english, row) in Translations.Table)
        {
            Assert.Equal(Translations.Languages.Length, row.Length, $"columns for '{english}'");
            foreach (var text in row)
                Assert.True(!string.IsNullOrWhiteSpace(text), $"empty translation for '{english}'");
        }
    }

    public static void EverySettingsTextIsTranslated()
    {
        // Labels, descriptions and section captions of Config.cs, read as text (no game DLLs here).
        var config = System.IO.File.ReadAllText(System.IO.Path.Combine(SourceDir(), "ClientPlugin", "Config.cs"));
        var texts = System.Text.RegularExpressions.Regex.Matches(config, @"(?:label|description):\s*""([^""]+)""|Separator\(""([^""]+)""\)");
        Assert.True(texts.Count > 20, $"found {texts.Count} texts");
        foreach (System.Text.RegularExpressions.Match m in texts)
        {
            var text = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
            Assert.True(Translations.Table.ContainsKey(text), $"no translation for '{text}'");
        }
    }

    public static void CultureMapping()
    {
        Assert.Equal("de", Translations.LanguageFor("de-DE"));
        Assert.Equal("pt-BR", Translations.LanguageFor("pt-BR"));
        Assert.Equal("zh-Hans", Translations.LanguageFor("zh-CN"));
        Assert.Equal("zh-Hant", Translations.LanguageFor("zh-TW"));
        Assert.Equal("zh-Hant", Translations.LanguageFor("zh-Hant"));
        Assert.Equal(null, Translations.LanguageFor("en-US"));
        Assert.Equal(null, Translations.LanguageFor(""));
        Assert.Equal("Allgemein", Translations.Get("General", "de"));
        Assert.Equal("General", Translations.Get("General", null), "English");
        Assert.Equal("Unknown text", Translations.Get("Unknown text", "ru"), "missing text stays English");
    }

    private static string SourceDir([System.Runtime.CompilerServices.CallerFilePath] string path = "") =>
        System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), ".."));
}
