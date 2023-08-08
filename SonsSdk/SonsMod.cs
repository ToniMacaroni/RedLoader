using System.Drawing;
using MelonLoader;

namespace SonsSdk;

public abstract class SonsMod : MelonTypeBase<SonsMod>
{
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
                $"Failed to register {MelonTypeName} '{Location}': Melon failed to initialize in the deprecated OnPreSupportModule callback!");
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
    public virtual void OnGameStart()
    {
        
    }

    #endregion

    #region Common Utils

    protected void Log(object msg, Color? color)
    {
        if(color.HasValue)
            LoggerInstance.Msg(color.Value, msg);
        else
            LoggerInstance.Msg(msg);
    }

    #endregion
}