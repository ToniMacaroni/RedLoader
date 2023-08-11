namespace MelonLoader;

public class CorePreferences
{
    public static CorePreferences Instance { get; private set; }
    
    public static ConfigCategory CoreCategory { get; private set; }
    
    public static ConfigEntry<bool> ShowConsole { get; private set; }

    private CorePreferences()
    {
        Instance = this;
        
        CoreCategory = ConfigSystem.CreateCategory("core", "Core");

        ShowConsole = CoreCategory.CreateEntry(
            "show_console",
            false,
            "Show Console",
            "Show the console.");
    }
    
    public static void Load()
    {
        if (Instance != null)
            return;
        
        Instance = new CorePreferences();
    }
}