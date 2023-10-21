using System.Collections;
using System.Drawing;
using System.Reflection;
using HarmonyLib;
using RedLoader;
using Sons.Cutscenes;
using Sons.Events;
using Sons.Gameplay.GameSetup;
using Sons.Gui.Options;
using Sons.Inventory;
using Sons.Loading;
using Sons.Save;
using TheForest.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Action = Il2CppSystem.Action;
using Color = UnityEngine.Color;
using Int32 = Il2CppSystem.Int32;
using Object = UnityEngine.Object;

namespace SonsSdk;

public static class SdkEvents
{
    /// <summary>
    /// Called when the player spawns in the world and gains control
    /// </summary>
    public static readonly MelonEvent OnGameStart = new();
    
    /// <summary>
    /// Called when the sdk has been fully initialized
    /// </summary>
    public static readonly MelonEvent OnSdkInitialized = new();
    
    /// <summary>
    /// Called on update when the player is in the world
    /// </summary>
    public static readonly MelonEvent OnInWorldUpdate = new();
    
    /// <summary>
    /// Called when the player picks up an item
    /// </summary>
    public static readonly MelonEvent<ItemInstance.ItemInstanceAndCount> OnItemPickup = new();
    
    /// <summary>
    /// Called when the player crafts an item
    /// </summary>
    public static readonly MelonEvent<int> OnItemCrafted = new();
    
    /// <summary>
    /// Called when the player consumes an item
    /// </summary>
    public static readonly MelonEvent<ItemInstance> OnItemConsumed = new();
    
    /// <summary>
    /// Called when the player equips some armor
    /// </summary>
    public static readonly MelonEvent OnArmorEquipped = new();

    /// <summary>
    /// Called by HDRP at the end of rendering a frame
    /// </summary>
    public static readonly MelonEvent<ScriptableRenderContext, Il2CppSystem.Collections.Generic.List<Camera>> OnCameraRender = new();
    
    /// <summary>
    /// Called before serializing the save game
    /// </summary>
    public static readonly MelonEvent BeforeSaveLoading = new();
    
    /// <summary>
    /// Called after serializing the save game.
    /// Returns if it should only save the player (i.e. saveGameType = MultiplayerClient)
    /// </summary>
    public static readonly MelonEvent<bool> AfterSaveLoading = new();
    
    /// <summary>
    /// Called before deserializing the save game
    /// </summary>
    public static readonly MelonEvent BeforeLoadSave = new();
    
    /// <summary>
    /// Called after deserializing the save game
    /// </summary>
    public static readonly MelonEvent AfterLoadSave = new();

    /// <summary>
    /// Called on the fith activation sequence when the game mode is initialized.
    /// This should be used for general world modifications.
    /// </summary>
    public static readonly MelonEvent OnGameActivated = new();

    public static readonly MelonEvent<ESonsScene> OnSonsSceneInitialized = new();

    internal static void Init()
    {
        if (_isInitialized)
        {
            return;
        }

        GlobalEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority.First);
        GlobalEvents.OnUpdate.Subscribe(OnUpdateInternal, Priority.First);
        
        EventRegistry.Register(GameEvent.ItemPickedUp, (EventRegistry.SubscriberCallback)SonsEventsOnItemPickedUp);
        EventRegistry.Register(GameEvent.LoadedMainScene, (EventRegistry.SubscriberCallback)SonsEventsOnGameStart);
        EventRegistry.Register(GameEvent.ItemCrafted, (EventRegistry.SubscriberCallback)SonsEventOnItemCrafted);
        EventRegistry.Register(GameEvent.ItemConsumed, (EventRegistry.SubscriberCallback)SonsEventOnItemConsumed);
        EventRegistry.Register(GameEvent.ArmorEquipped, (EventRegistry.SubscriberCallback)SonsEventOnArmorEquipped);

        RenderPipelineManager.endContextRendering += (Il2CppSystem.Action<ScriptableRenderContext, Il2CppSystem.Collections.Generic.List<Camera>>)OnEndContextRendering;

        _isInitialized = true;
    }

    private static void OnSceneWasInitialized(int sceneIdx, string sceneName)
    {
        switch (sceneName)
        {
            case TitleSceneName:
                MainInitializer.InitTitleScene();
                OnSonsSceneInitialized.Invoke(ESonsScene.Title);
                break;
            case LoadingSceneName:
                OnSonsSceneInitialized.Invoke(ESonsScene.Loading);
                break;
            case GameSceneName:
                RLog.Msg(System.Drawing.Color.DeepPink, $"=== Game Scene Initialized ===");
                LoadSave.OnGameStart += (Action)OnGameActivation;
                OnSonsSceneInitialized.Invoke(ESonsScene.Game);
                break;
        }
    }

    internal static void RegisterSaveGameManager()
    {
        SaveGameManager.RegisterBeforeLoadCallback((Action)OnBeforeLoadSave);
        SaveGameManager.RegisterAfterLoadCallback((Action)OnAfterLoadSave);
        SaveGameManager.RegisterBeforeSaveCallback((Action)OnBeforeSave);
        SaveGameManager.RegisterAfterSaveCallback((Action<bool>)OnAfterSave);
    }

    private static void OnUpdateInternal()
    {
        ModInputCache.CheckAll();
        
        if (!LocalPlayer.IsInWorld)
        {
            return;
        }
        
        OnInWorldUpdate.Invoke();
    }

    #region Game Events

    private static void SonsEventsOnItemPickedUp(Il2CppSystem.Object o)
    {
        var item = o.TryCast<ItemInstance.ItemInstanceAndCount>();
        if (item == null)
        {
            return;
        }
        
        OnItemPickup.Invoke(item);
    }
    
    private static void SonsEventOnItemConsumed(Il2CppSystem.Object o)
    {
        var item = o.TryCast<ItemInstance>();
        if (item == null)
        {
            return;
        }
        
        OnItemConsumed.Invoke(item);
    }
    
    private static void SonsEventOnArmorEquipped(object _)
    {
        OnArmorEquipped.Invoke();
    }
    
    private static void SonsEventOnItemCrafted(Il2CppSystem.Object o)
    {
        var id = o.Unbox<int>();
        OnItemCrafted.Invoke(id);
    }
    
    private static void SonsEventsOnGameStart(object o)
    {
        OnGameStart.Invoke();
        ItemTools.ItemHookAdder.Flush();
    }
    
    private static void OnEndContextRendering(ScriptableRenderContext context, Il2CppSystem.Collections.Generic.List<Camera> cameras){
        OnCameraRender.Invoke(context, cameras);
    }
    
    private static void OnBeforeSave()
    {
        RLog.Debug($"ON_BEFORE_SAVE_LOADING");
        BeforeSaveLoading.Invoke();
    }
    
    private static void OnAfterSave(bool savePlayerOnly)
    {
        RLog.Debug($"ON_AFTER_SAVE_LOADING");
        AfterSaveLoading.Invoke(savePlayerOnly);
    }
    
    private static void OnBeforeLoadSave()
    {
        RLog.Debug($"ON_BEFORE_LOAD_SAVE");
        BeforeLoadSave.Invoke();
        GameState.LastLoadedSaveId = GameSetupManager.GetSelectedSaveId();
    }
    
    private static void OnAfterLoadSave()
    {
        RLog.Debug($"ON_AFTER_LOAD_SAVE");
        AfterLoadSave.Invoke();
    }

    private static void OnGameActivation()
    {
        RLog.Msg(System.Drawing.Color.DeepPink, $"||| Game Activated |||");
        OnGameActivated.Invoke();
    }

    #endregion

    private const string TitleSceneName = "SonsTitleScene";
    private const string LoadingSceneName = "SonsMainLoading";
    private const string GameSceneName = "SonsMain";

    private static bool _isInitialized;
}