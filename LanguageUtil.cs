using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UObject = UnityEngine.Object;

namespace Core.LanguageUtil;

/// <summary>
///     Utils specifically for interacting with language strings.
/// </summary>
public static class LanguageUtil
{
    private static readonly Dictionary<Language.LanguageCode, Dictionary<string, Dictionary<string, string>>> LanguageStringDictionary = new();
    private static readonly Dictionary<string, Language.LanguageCode> FallbackLanguagePerSheet = new();
    private static readonly Dictionary<string, Func<Language.LanguageCode>> LanguageSourcePerSheet = new();

    static LanguageUtil()
    {
        Logger.LogFine("[Core][LanguageUtil] - Hooking Modding.ModHooks.LanguageGetHook!");
        Modding.ModHooks.LanguageGetHook += ModHooksOnLanguageGetHook;
    }

    /// <summary>
    ///     Adds a language string.
    /// </summary>
    /// <param name="message">The string.</param>
    /// <param name="key">The key of the string.</param>
    /// <param name="sheet">The sheet the key is contained in.</param>
    /// <param name="language">The language for the given key (default english).</param>
    [PublicAPI]
    public static void AddString(string message, string key, string sheet, Language.LanguageCode language = Language.LanguageCode.EN)
    {
        if (!LanguageStringDictionary.ContainsKey(language)) LanguageStringDictionary[language] = new Dictionary<string, Dictionary<string, string>>();
        if (!LanguageStringDictionary[language].ContainsKey(sheet)) LanguageStringDictionary[language][sheet] = new Dictionary<string, string>();
        Logger.LogDebug($"[Core][LanguageUtil] - Adding string '{language}'/'{sheet}'/'{key}'!");
        LanguageStringDictionary[language][sheet][key] = message;
    }

    /// <summary>
    ///     Adds a source of a Language.LanguageCode for a given sheet.
    /// </summary>
    /// <param name="sheet">The sheet the languageSource is for.</param>
    /// <param name="fallbackLanguage">The language that is used when the current language does not contain the key.</param>
    [PublicAPI]
    public static void AddFallbackLanguageForSheet(string sheet, Language.LanguageCode fallbackLanguage)
    {
        Logger.LogDebug($"[Core][LanguageUtil] - Adding fallback language '{fallbackLanguage}' to '{sheet}'!");
        FallbackLanguagePerSheet[sheet] = fallbackLanguage;
    }

    /// <summary>
    ///     Adds a source of a Language.LanguageCode for a given sheet.
    /// </summary>
    /// <param name="sheet">The sheet the languageSource is for.</param>
    /// <param name="languageSource">The method that is used to get the language for the strings.</param>
    [PublicAPI]
    public static void AddLanguageSourceForSheet(string sheet, Func<Language.LanguageCode> languageSource)
    {
        Logger.LogDebug($"[Core][LanguageUtil] - Adding language source '{languageSource.Method}' to '{sheet}'!");
        LanguageSourcePerSheet[sheet] = languageSource;
    }

    private static string ModHooksOnLanguageGetHook(string key, string sheet, string orig)
    {
        return GetString(GetLanguage(sheet), key, sheet, orig);
    }

    private static Language.LanguageCode GetLanguage(string sheet)
    {
        Logger.LogFine($"[Core][LanguageUtil] - Looking up language for sheet '{sheet}'...");
        if (!LanguageSourcePerSheet.ContainsKey(sheet)) return Language.Language.CurrentLanguage();
        Language.LanguageCode language = LanguageSourcePerSheet[sheet]();
        if (LanguageStringDictionary.ContainsKey(language) && LanguageStringDictionary[language].ContainsKey(sheet)) return language;
        return FallbackLanguagePerSheet[sheet];
    }

    private static string GetString(Language.LanguageCode language, string key, string sheet, string orig)
    {
        Logger.LogFine($"[Core][LanguageUtil] - Looking up string for '{language}'/'{sheet}'/'{key}' with orig '{orig}'...");
        if (!HasSheet(language, sheet)) return orig;
        return LanguageStringDictionary[language][sheet].ContainsKey(key) ? LanguageStringDictionary[language][sheet][key] : orig;
    }

    private static bool HasSheet(Language.LanguageCode language, string sheet)
    {
        Logger.LogFine($"[Core][LanguageUtil] - Checking if '{language}'/'{sheet}' exists...");
        return LanguageStringDictionary.ContainsKey(language) && LanguageStringDictionary[language].ContainsKey(sheet);
    }
}