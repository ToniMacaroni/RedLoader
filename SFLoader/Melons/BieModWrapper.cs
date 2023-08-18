using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace SFLoader;

#pragma warning disable 0618
public class BieModWrapper : MelonMod
{
    private BasePlugin _plugin;
    
    public BieModWrapper(BasePlugin plugin)
    {
        _plugin = plugin;
    }

    public override void OnInitializeMod()
    {
        //MelonLogger.Msg($"_________________ Initializing {Info.Name} v{Info.Version} by {Info.Author} ___________________");
        
        _plugin.Config = new ConfigFile(Path.Combine(Paths.ConfigPath, Info.Name + ".cfg"), true);
        _plugin.Log = new ManualLogSource(Info.Name);
        _plugin.Log.LogEvent += OnLogEvent;
        
        _plugin.Load();
    }

    private void OnLogEvent(object sender, LogEventArgs args)
    {
        LoggerInstance.Msg(args.Data);
    }
}