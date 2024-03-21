using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace SonsSdk;

public static class LocalizationTools
{
    public static StringTable BindingsTable => LocalizationSettings.StringDatabase.GetTable("Bindings");
    public static StringTable BlueprintBookTable => LocalizationSettings.StringDatabase.GetTable("BlueprintBook");
    public static StringTable CookingTable => LocalizationSettings.StringDatabase.GetTable("Cooking");
    public static StringTable HintsTable => LocalizationSettings.StringDatabase.GetTable("Hints");
    public static StringTable ItemsTable => LocalizationSettings.StringDatabase.GetTable("Items");
    public static StringTable MenusTable => LocalizationSettings.StringDatabase.GetTable("Menus");
    public static StringTable MenuTooltipsTable => LocalizationSettings.StringDatabase.GetTable("MenuTooltips");
    public static StringTable RobbyTable => LocalizationSettings.StringDatabase.GetTable("Robby");
    public static StringTable ScreenPromptsTable => LocalizationSettings.StringDatabase.GetTable("ScreenPrompts");
    public static StringTable SettingsTooltipsTable => LocalizationSettings.StringDatabase.GetTable("SettingsTooltips");
    public static StringTable SubtitlesTable => LocalizationSettings.StringDatabase.GetTable("Subtitles");
    public static StringTable TutorialsTable => LocalizationSettings.StringDatabase.GetTable("Tutorials");
}