using Il2CppInterop.Runtime.Injection;
using RedLoader;
using Sons.Gui.Options;
using Sons.Loading;
using SonsSdk;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SUI;

/// <summary>
/// class for attaching custom SUI UIs to game menus
/// </summary>
public static class MenuAttacher
{
    private static readonly Dictionary<EHookLocation, BaseAttacher> _attachers = new();
    
    private static bool _initialized;
    
    public static void AddContainer(SContainerOptions container, EHookLocation location)
    {
        Initialize();
        
        if (!_attachers.TryGetValue(location, out var attacher))
        {
            throw new Exception("No attacher found for location " + location);
        }
        
        attacher.AddChild(container.Root);
    }
    
    private static void Initialize()
    {
        if (_initialized)
            return;
        
        _initialized = true;

        _attachers[EHookLocation.Options] = new GameObject(nameof(OptionsAttacher)).AddComponent<OptionsAttacher>();
    }

    public enum EHookLocation
    {
        Options,
    }
}

public class BaseAttacher : MonoBehaviour
{
    static BaseAttacher()
    {
        ClassInjector.RegisterTypeInIl2Cpp<BaseAttacher>();
    }

    private void Awake()
    {
        SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)OnSceneLoaded);
        SceneManager.add_sceneUnloaded((UnityAction<Scene>)OnSceneUnloaded);
    }

    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
    
    protected virtual void OnSceneUnloaded(Scene scene)
    {
    }
    
    public virtual void Persist() => DontDestroyOnLoad(gameObject);

    public virtual void Attach()
    { }
    
    public void AddChild(GameObject child)
    {
        child.transform.SetParent(transform, false);
    }
}

public class OptionsAttacher : BaseAttacher
{
    static OptionsAttacher()
    {
        ClassInjector.RegisterTypeInIl2Cpp<OptionsAttacher>();
    }

    protected override void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == SonsSceneManager.OptionsMenuSceneName)
        {
            RLog.Debug("Scene being unloaded, persisting options attacher");
            Persist();
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SonsSceneManager.OptionsMenuSceneName)
        {
            Attach();
        }
    }

    public override void Attach()
    {
    }
}