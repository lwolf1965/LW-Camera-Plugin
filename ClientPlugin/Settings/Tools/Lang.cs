using System;
using ClientPlugin.Policy;
using Keen.VRage.Library.Localization;
using Keen.VRage.Library.Utils;

namespace ClientPlugin.Settings.Tools;

/// <summary>
/// Settings texts in the game's current language (Policy/Translations); English when the language or a text is missing.
/// Game API: Singleton&lt;LocalizationManager&gt;.Instance.CurrentCulture (a StringId such as "en-US").
/// </summary>
internal static class Lang
{
    private static string loggedCulture;

    public static string T(string english) => Translations.Get(english, Current());

    private static string Current()
    {
        try
        {
            var culture = Singleton<LocalizationManager>.Instance?.CurrentCulture.String;
            var language = Translations.LanguageFor(culture);
            if (culture != loggedCulture)
            {
                loggedCulture = culture;
                Game.PluginLog.Info($"Settings language: game culture '{culture}' -> {language ?? "English"}");
            }
            return language;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
