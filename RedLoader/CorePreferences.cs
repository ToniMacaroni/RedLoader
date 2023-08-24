namespace RedLoader;

public class CorePreferences
{
    public static CorePreferences Instance { get; private set; }
    
    public static ConfigCategory CoreCategory { get; private set; }
    
    public static ConfigEntry<bool> ShowConsole { get; private set; }
    
    public static ConfigEntry<bool> HideStatusWindow { get; private set; }
    
    public static ConfigEntry<FConsoleRect> ConsoleRect { get; private set; }
    
    public static ConfigEntry<bool> SaveConsoleRect { get; private set; }

    private CorePreferences()
    {
        Instance = this;
        
        CoreCategory = ConfigSystem.CreateCategory("core", "Core");

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
        
        SaveConsoleRect = CoreCategory.CreateEntry(
            "save_console_rect",
            false,
            "Save Console Rect",
            "Save the console rect when closing the game.");
    }
    
    public static void Load()
    {
        if (Instance != null)
            return;
        
        Instance = new CorePreferences();
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