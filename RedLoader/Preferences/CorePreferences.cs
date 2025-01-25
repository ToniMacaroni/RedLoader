using System.IO;
using RedLoader.Utils;

namespace RedLoader;

public class CorePreferences
{
    public static ConfigCategory CoreCategory { get; private set; }
    
    public static ConfigEntry<bool> ShowConsole { get; private set; }

    public static ConfigEntry<bool> HideStatusWindow { get; private set; }
    
    public static ConfigEntry<FConsoleRect> ConsoleRect { get; private set; }
    
    // public static ConfigEntry<bool> SaveConsoleRect { get; private set; }
    
    public static ConfigEntry<bool> RedirectUnityLogs { get; private set; }
    
    /// <summary>
    /// Makes the exceptions more readable.
    /// </summary>
    /// <config>Readable Exceptions</config>
    /// <type>bool</type>
    public static ConfigEntry<bool> ReadableExceptions { get; private set; }
    
    /// <summary>
    /// Disable the popup notifications.
    /// </summary>
    /// <config>Disable Notifications</config>
    /// <type>bool</type>
    public static ConfigEntry<bool> DisableNotifications { get; private set; }

    /// <summary>
    /// Automatically rename dxgi.dll in the game folder to make Redloader able to load Reshade.
    /// </summary>
    /// <config>Auto Fix Reshade</config>
    /// <type>bool</type>
    public static ConfigEntry<bool> AutoFixReshade { get; private set; }

    public static void Init()
    {
        CoreCategory = ConfigSystem.CreateCategory("core", "Core");
        CoreCategory.SetFilePath(Path.Combine(LoaderEnvironment.UserDataDirectory, "_Redloader.cfg"), false, false);

        ShowConsole = CoreCategory.CreateEntry(
            "show_console",
            false,
            "Show Console",
            "Show the console.");
        
        HideStatusWindow = CoreCategory.CreateEntry(
            "hide_status_window",
            false,
            "Hide Status Window",
            "Hide the sf loader status window.");
        
        ConsoleRect = CoreCategory.CreateEntry(
            "console_rect",
            new FConsoleRect(0, 0, 1000, 700),
            "Console Rect",
            "The position and size of the console. Gets saved when closing the game");
        
        // SaveConsoleRect = CoreCategory.CreateEntry(
        //     "save_console_rect",
        //     false,
        //     "Save Console Rect",
        //     "Save the console rect when closing the game.");
        
        RedirectUnityLogs = CoreCategory.CreateEntry(
           "redirect_unity_logs",
           false,
           "Redirect Unity Logs",
           "Redirect Unity logs so they show up in the console and log files.");
        
        ReadableExceptions = CoreCategory.CreateEntry(
            "readable_exceptions",
            false,
            "Readable Exceptions",
            "Makes the exceptions more readable (only use for debugging).");
        
        DisableNotifications = CoreCategory.CreateEntry(
            "disable_notifications",
            false,
            "Disable Notifications",
            "Disable the popup notifications.");
        
        AutoFixReshade = CoreCategory.CreateEntry(
            "auto_fix_reshade",
            true,
            "Auto Fix Reshade",
            "Automatically rename dxgi.dll in the game folder to make Redloader able to load Reshade.");
        
        CoreCategory.LoadFromFile(false);
    }
    
    internal static void SaveConsoleRect()
    {
        ConsoleRect.Value = ConsoleManager.GetConsoleRect();
    }
    
    public struct FConsoleRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        
        public FConsoleRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
