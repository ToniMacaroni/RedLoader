using System.Reflection;
using HarmonyLib;
using RedLoader;
using MonoMod.Utils;
using RedLoader.Utils;
using SonsSdk.Attributes;
using TheForest;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SonsSdk;

public abstract class SonsMod : ModTypeBase<SonsMod>
{
    public ManifestData Manifest { get; internal set; }

    internal List<AssetBundleAttribute> AssetBundleAttrs = new();

    internal ModConfigurator Configurator = new();
    
    public PathObject DataPath { get; internal set; }

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
        DataPath = LoaderEnvironment.GetModDataPath(this);
        
        try
        {
            OnPreSupportModule();
        }
        catch (Exception ex)
        {
            RLog.Error(
                $"Failed to register {MelonTypeName} '{MelonAssembly.Location}': Melon failed to initialize in the deprecated OnPreSupportModule callback!");
            RLog.Error(ex.ToString());
            return false;
        }

        if (!base.RegisterInternal())
        {
            return false;
        }

        if (GlobalEvents.MelonHarmonyInit.Disposed)
        {
            HarmonyInit();
        }
        else
        {
            GlobalEvents.MelonHarmonyInit.Subscribe(HarmonyInit, Priority, true);
        }

        Configure(Configurator);

        return true;
    }

    private protected override void RegisterCallbacks()
    {
        base.RegisterCallbacks();

        GlobalEvents.OnSceneWasLoaded.Subscribe(OnSceneWasLoaded, Priority);
        GlobalEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority);
        GlobalEvents.OnSceneWasUnloaded.Subscribe(OnSceneWasUnloaded, Priority);
        SdkEvents.OnGameStart.Subscribe(OnGameStart, Priority);
        SdkEvents.OnSdkInitialized.Subscribe(OnSdkInitialized, Priority);
        SdkEvents.OnSonsSceneInitialized.Subscribe(OnSonsSceneInitialized, Priority);
        
        SdkEvents.OnGameStart.Subscribe(RegisterCommands, Priority);
    }

    #region Callbacks

    /// <summary>
    /// Configure mod settings and subscribe to events.
    /// </summary>
    /// <param name="config"></param>
    public virtual void Configure(ModConfigurator config)
    { }

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

    protected void StartPropDebugging()
    {
        MemberDebugger.StartDebugging(this);
    }

    protected void StopDebugging()
    {
        MemberDebugger.StopDebugging(this);
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

    protected void Log(object obj) => LoggerInstance.Msg(obj);
    protected void Log(Color color, object obj) => LoggerInstance.Msg(color, obj);

    public class ModConfigurator
    {
        public ModConfigurator SubscribeOnWorldUpdate(LemonAction action)
        {
            SdkEvents.OnInWorldUpdate.Subscribe(action);
            return this;
        }
    }

    #endregion
}