using System.Collections;
using System.Drawing;
using System.Reflection;
using Harmony;
using HarmonyLib;
using RedLoader;
using Sons.Ai.Vail;
using Sons.Cutscenes;
using Sons.Events;
using Sons.Gameplay.GameSetup;
using Sons.Gui;
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
using Priority = HarmonyLib.Priority;

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
    /// Called on the first <see cref="OnInWorldUpdate"/> tick
    /// </summary>
    public static readonly MelonEvent OnAfterSpawn = new();
    
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
    /// Called before serializing the save game (i.e. saving a save)
    /// </summary>
    public static readonly MelonEvent BeforeSaveLoading = new();
    
    /// <summary>
    /// Called after serializing the save game. (i.e. saving a save)
    /// Returns if it should only save the player (i.e. saveGameType = MultiplayerClient)
    /// </summary>
    public static readonly MelonEvent<bool> AfterSaveLoading = new();
    
    /// <summary>
    /// Called before deserializing the save game (i.e. loading a save)
    /// </summary>
    public static readonly MelonEvent BeforeLoadSave = new();
    
    /// <summary>
    /// Called after deserializing the save game (i.e. loading a save)
    /// </summary>
    public static readonly MelonEvent AfterLoadSave = new();

    /// <summary>
    /// Called on the fith activation sequence when the game mode is initialized.
    /// This should be used for general world modifications.
    /// </summary>
    public static readonly MelonEvent OnGameActivated = new();

    /// <summary>
    /// Called when a world sim actor has been added to the world
    /// </summary>
    public static readonly MelonEvent<WorldSimActor> OnWorldSimActorAdded = new();
    
    /// <summary>
    /// Called when a world sim actor has been removed from the world
    /// </summary>
    public static readonly MelonEvent<WorldSimActor> OnWorldSimActorRemoved = new();

    public static readonly MelonEvent<ESonsScene> OnSonsSceneInitialized = new();

    /// <summary>
    /// Called when the player exits to main menu
    /// </summary>
    public static readonly MelonEvent OnWorldExited = new();
    
    public static readonly MelonEvent<PauseMenu> OnPauseMenuOpened = new();
    public static readonly MelonEvent<PauseMenu> OnPauseMenuClosed = new();

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
        
        Patches.Patch();
        SavingCallbackPatches.Patch();

        _isInitialized = true;
    }

    private static void OnSceneWasInitialized(int sceneIdx, string sceneName)
    {
        switch (sceneName)
        {
            case TitleSceneName:
                MainInitializer.InitTitleScene();
                _onAfterSpawnCalled = false;
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

    private static void OnUpdateInternal()
    {
        ModInputCache.CheckAll();
        
        if (!LocalPlayer.IsInWorld)
        {
            return;
        }

        if (!_onAfterSpawnCalled)
        {
            _onAfterSpawnCalled = true;
            OnAfterSpawn.Invoke();
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
    private static bool _onAfterSpawnCalled;

    private class Patches
    {
        private static ConfiguredPatcher<Patches> _patcher;

        public static void Patch()
        {
            _patcher = new ConfiguredPatcher<Patches>(Core.HarmonyInstance);
            _patcher.Patch(nameof(AddWorldSimActor));
            _patcher.Patch(nameof(RemoveWorldSimActor));
            _patcher.Patch(nameof(OpenPauseMenu));
            _patcher.Patch(nameof(ClosePauseMenu));
            _patcher.Patch(nameof(RunQuittingGameCallbacks));
        }
        
        [HarmonyPatch(typeof(VailWorldSimulation), nameof(VailWorldSimulation.AddActor))]
        private static void AddWorldSimActor(WorldSimActor actor, bool onLoad)
        {
            OnWorldSimActorAdded.Invoke(actor);
        }

        [HarmonyPatch(typeof(VailWorldSimulation), nameof(VailWorldSimulation.RemoveActor))]
        private static void RemoveWorldSimActor(WorldSimActor removeActor)
        {
            OnWorldSimActorRemoved.Invoke(removeActor);
        }
        
        [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Open))]
        private static void OpenPauseMenu(PauseMenu __instance)
        {
            OnPauseMenuOpened.Invoke(__instance);
        }
        
        [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Close))]
        private static void ClosePauseMenu(PauseMenu __instance)
        {
            OnPauseMenuClosed.Invoke(__instance);
        }
        
        [HarmonyPatch(typeof(Sons.Save.GameState), nameof(Sons.Save.GameState.RunQuittingGameCallbacks))]
        private static void RunQuittingGameCallbacks()
        {
            OnWorldExited.Invoke();
        }
    }
    
    public class SavingCallbackPatches
    {
        [HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.Load), typeof(string), typeof(SaveGameType))]
        [HarmonyPrefix]
        public static void BeforeLoad(string dir, SaveGameType saveGameType)
        {
            if (!SaveGameManager.HasInstance)
            {
                RLog.Error("SaveGameManager not initialized, aborting load callback.");
                return;
            }
            
            // RLog.Msg($"Loading savegame from {dir} of type {saveGameType}");

            GameState.LastLoadedSaveId = GameSetupManager.GetSelectedSaveId();
            BeforeLoadSave.Invoke();
        }
        
        [HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.Load), typeof(string), typeof(SaveGameType))]
        [HarmonyPostfix]
        public static void AfterLoad(string dir, SaveGameType saveGameType)
        {
            if (!SaveGameManager.HasInstance)
            {
                return;
            }
            
            // RLog.Msg($"AfterLoad");
            
            AfterLoadSave.Invoke();
        }
        
        [HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.Save), typeof(string), typeof(string), typeof(bool))]
        [HarmonyPrefix]
        public static void BeforeSave(string dir, string gameName, bool savePlayerOnly)
        {
            if (!SaveGameManager.HasInstance)
            {
                RLog.Error("SaveGameManager not initialized, aborting save callback.");
                return;
            }
            
            // RLog.Msg($"Saving savegame to {dir} with name {gameName} (SavePlayerOnly:{savePlayerOnly})");
            
            BeforeSaveLoading.Invoke();
        }
        
        [HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.Save), typeof(string), typeof(string), typeof(bool))]
        [HarmonyPrefix]
        public static void AfterSave(string dir, string gameName, bool savePlayerOnly)
        {
            if (!SaveGameManager.HasInstance)
            {
                return;
            }
            
            // RLog.Msg($"AfterSave");
            
            AfterSaveLoading.Invoke(savePlayerOnly);
        }

        public static void Patch()
        {
            Core.HarmonyInstance.PatchAll(typeof(SavingCallbackPatches));
        }
    }
}