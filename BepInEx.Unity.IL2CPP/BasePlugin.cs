using BepInEx.Configuration;
using BepInEx.Logging;

namespace BepInEx.Unity.IL2CPP;

public abstract class BasePlugin
{
    public ManualLogSource Log { get; set; }
    
    public ConfigFile Config { get; set; }
    
    public abstract void Load();
}