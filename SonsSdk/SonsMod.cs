using System.Drawing;
using System.Reflection;
using HarmonyLib;
using RedLoader;
using MonoMod.Utils;
using SonsSdk.Attributes;
using TheForest;

namespace SonsSdk;

public abstract class SonsMod : MelonTypeBase<SonsMod>
{
    public ManifestData Manifest { get; internal set; }
    
    static SonsMod()
    {
        TypeName = "SonsMod";
    }

    private void HarmonyInit()
    {
        if (!MelonAssembly.HarmonyDontPatchAll)
        {
            HarmonyInstance.PatchAll(MelonAssembly.Assembly);
        }
    }

    private protected override bool RegisterInternal()
    {
        try
        {
            OnPreSupportModule();
        }
        catch (Exception ex)
        {
            MelonLogger.Error(
                $"Failed to register {MelonTypeName} '{MelonAssembly.Location}': Melon failed to initialize in the deprecated OnPreSupportModule callback!");
            MelonLogger.Error(ex.ToString());
            return false;
        }

        if (!base.RegisterInternal())
        {
            return false;
        }

        if (MelonEvents.MelonHarmonyInit.Disposed)
        {
            HarmonyInit();
        }
        else
        {
            MelonEvents.MelonHarmonyInit.Subscribe(HarmonyInit, Priority, true);
        }

        return true;
    }

    private protected override void RegisterCallbacks()
    {
        base.RegisterCallbacks();

        MelonEvents.OnSceneWasLoaded.Subscribe(OnSceneWasLoaded, Priority);
        MelonEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority);
        MelonEvents.OnSceneWasUnloaded.Subscribe(OnSceneWasUnloaded, Priority);
        SdkEvents.OnGameStart.Subscribe(OnGameStart, Priority);
        SdkEvents.OnSdkInitialized.Subscribe(OnSdkInitialized, Priority);
        SdkEvents.OnSonsSceneInitialized.Subscribe(OnSonsSceneInitialized, Priority);
        
        SdkEvents.OnGameStart.Subscribe(RegisterCommands, Priority);
    }

    #region Callbacks

    /// <summary>
    ///     Runs when a new Scene is loaded.
    /// </summary>
    public virtual void OnSceneWasLoaded(int buildIndex, string sceneName)
    { }

    /// <summary>
    ///     Runs once a Scene is initialized.
    /// </summary>
    public virtual void OnSceneWasInitialized(int buildIndex, string sceneName)
    { }

    /// <summary>
    ///     Runs once a Scene unloads.
    /// </summary>
    public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName)
    { }

    /// <summary>
    /// Runs when the game scene is loaded (right before the player gains control).
    /// </summary>
    protected virtual void OnGameStart()
    { }
    
    /// <summary>
    /// Runs when the SDK is fully initialized. SDK usage like creation of custom UI should be done here.
    /// </summary>
    protected virtual void OnSdkInitialized()
    { }
    
    /// <summary>
    /// Runs when a scene is initialized. But with an enum parameter to check for sons scenes.
    /// </summary>
    /// <param name="sonsScene"></param>
    protected virtual void OnSonsSceneInitialized(SdkEvents.ESonsScene sonsScene)
    {}

    #endregion

    #region Common Utils

    protected void Log(object msg, Color? color = null)
    {
        if(color.HasValue)
            LoggerInstance.Msg(color.Value, msg);
        else
            LoggerInstance.Msg(msg);
    }

    protected void RegisterCommand(string command, Func<string, bool> callback)
    {
        DebugConsole.RegisterCommand(command, callback, DebugConsole.Instance);
    }

    private void RegisterCommands()
    {
        Type targetType = GetType();
        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (MethodInfo method in methods)
        {
            DebugCommandAttribute attribute = method.GetCustomAttribute<DebugCommandAttribute>();
            if (attribute != null)
            {
                var fastDelegate = method.GetFastDelegate();

                bool Wrapper(string s)
                {
                    var ret = fastDelegate.Invoke(this, s);
                    if (ret is bool b)
                        return b;
                    return true;
                }
                
                DebugConsole.RegisterCommand(attribute.Command, (Il2CppSystem.Func<string, bool>)Wrapper, DebugConsole.Instance);
                
                LoggerInstance.Msg($"Registered command '{attribute.Command}'");
            }
        }
    }

    #endregion
}