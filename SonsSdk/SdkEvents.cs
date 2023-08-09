using System.Collections;
using System.Drawing;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using Sons.Cutscenes;
using Sons.Events;
using Sons.Gui.Options;
using Sons.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SonsSdk;

public static class SdkEvents
{
    public static readonly MelonEvent OnGameStart = new();
    public static readonly MelonEvent OnSdkInitialized = new();
    
    public static readonly MelonEvent<ESonsScene> OnSonsSceneInitialized = new();

    internal static void Init()
    {
        if (_isInitialized)
        {
            return;
        }
        
        Patches.InitPatches();
        
        MelonEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority.First);
        
        _isInitialized = true;
    }

    private static IEnumerator DelayedTitleLoad()
    {
        SceneManager.LoadScene(SonsSceneManager.OptionsMenuSceneName, LoadSceneMode.Additive);
        yield return null;
        SUI.SUI.InitPrefabs();
        SceneManager.UnloadScene(SonsSceneManager.OptionsMenuSceneName);
        
        OnSdkInitialized.Invoke();
    }
    
    private static void OnSceneWasInitialized(int sceneIdx, string sceneName)
    {
        switch (sceneName)
        {
            case TitleSceneName:
                DelayedTitleLoad().Run();
                OnSonsSceneInitialized.Invoke(ESonsScene.Title);
                break;
            case LoadingSceneName:
                OnSonsSceneInitialized.Invoke(ESonsScene.Loading);
                break;
            case GameSceneName:
                OnSonsSceneInitialized.Invoke(ESonsScene.Game);
                break;
        }
    }
    
    public enum ESonsScene
    {
        Title,
        Loading,
        Game
    }
    
    private const string TitleSceneName = "SonsTitleScene";
    private const string LoadingSceneName = "SonsMainLoading";
    private const string GameSceneName = "SonsMain";

    private static bool _isInitialized;
}