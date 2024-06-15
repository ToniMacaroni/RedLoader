using System.Reflection;
using HarmonyLib;
using RedLoader;
using MonoMod.Utils;
using RedLoader.Utils;
using SonsSdk.Attributes;
using SUI;
using TheForest;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SonsSdk;

public abstract class SonsMod : ModTypeBase<SonsMod>
{
    public ManifestData Manifest { get; internal set; }

    internal List<AssetBundleAttribute> AssetBundleAttrs = new();

    /// <summary>
    /// Method that gets called on update but only when in the world.
    /// </summary>
    [Obsolete("Use the IOnUpdateReceiver interface instead")]
    protected LemonAction OnWorldUpdatedCallback;
    
    /// <summary>
    /// Gets called after all commands have been registered.
    /// </summary>
    protected LemonAction OnCommandsRegisteredCallback;
    
    public PathObject DataPath { get; internal set; }

    static SonsMod()
    {
        TypeName = "SonsMod";
    }

    private void HarmonyInit()
    {
        if (HarmonyPatchAll)
        {
            HarmonyInstance.PatchAll(ModAssembly.Assembly);
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
                $"Failed to register {MelonTypeName} '{ModAssembly.Location}': Melon failed to initialize in the deprecated OnPreSupportModule callback!");
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

        if(OnWorldUpdatedCallback != null)
            SdkEvents.OnInWorldUpdate.Subscribe(OnWorldUpdatedCallback, Priority);
        
        if (this is IOnUpdateReceiver onUpdateReceiver)
            GlobalEvents.OnUpdate.Subscribe(onUpdateReceiver.OnUpdate, Priority);
        
        if (this is IOnInWorldUpdateReceiver onInWorldUpdateReceiver)
            SdkEvents.OnInWorldUpdate.Subscribe(onInWorldUpdateReceiver.OnInWorldUpdate, Priority);
        
        if (this is IOnAfterSpawnReceiver onAfterSpawnReceiver)
            SdkEvents.OnAfterSpawn.Subscribe(onAfterSpawnReceiver.OnAfterSpawn, Priority);
        
        if (this is IOnGameActivatedReceiver onGameActivatedReceiver)
            SdkEvents.OnGameActivated.Subscribe(onGameActivatedReceiver.OnGameActivated, Priority);
        
        if (this is IOnBeforeLoadSaveReceiver onBeforeLoadSaveReceiver)
            SdkEvents.BeforeLoadSave.Subscribe(onBeforeLoadSaveReceiver.OnBeforeLoadSave, Priority);
        
        if (this is IOnAfterLoadSaveReceiver onAfterLoadSaveReceiver)
            SdkEvents.AfterLoadSave.Subscribe(onAfterLoadSaveReceiver.OnAfterLoadSave, Priority);

        SdkEvents.OnGameStart.Subscribe(RegisterCommands, Priority);
    }
    
    internal Dictionary<string, Action<SBgButtonOptions>> GetModPanelActions()
    {
        var actions = new Dictionary<string, Action<SBgButtonOptions>>();
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<ModPanelActionAttribute>();
            if (attr == null)
                continue;

            var action = (Action<SBgButtonOptions>) Delegate.CreateDelegate(typeof(Action<SBgButtonOptions>), this, method);
            actions[attr.Name] = action;
        }

        return actions;
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
    protected virtual void OnSonsSceneInitialized(ESonsScene sonsScene)
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
        
        OnCommandsRegisteredCallback?.Invoke();
    }

    protected void Log(object obj) => LoggerInstance.Msg(obj);
    protected void Log(Color color, object obj) => LoggerInstance.Msg(color, obj);

    protected void MapResourceBundle(string bundleName, Type type)
    {
        AssetLoaders.MapBundleToFile(AssetLoaders.LoadDataFromAssembly(ModAssembly.Assembly, $"Resources.{bundleName}"), type);
    }

    #endregion

    #region ModReports

    internal static List<ModReportInfo> ModReports = new();
    
    internal static void ReportMod(string modId, string message)
    {
        ModReports.Add(new ModReportInfo()
        {
            ModId = modId,
            Message = message
        });
    }

    internal class ModReportInfo
    {
        public string ModId;
        public string Message;
    }

    #endregion
    
}