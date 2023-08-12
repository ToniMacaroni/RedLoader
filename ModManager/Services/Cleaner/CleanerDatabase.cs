namespace ModManager;

internal static class CleanerDatabase
{
    public static BieCleaner BieCleaner = new();
    public static MelonCleaner MelonCleaner = new();
    public static PartialSfLoaderCleaner PartialSfLoaderCleaner = new();
    
    public static UeCleaner UeCleaner = new();
}