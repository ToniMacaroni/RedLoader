using RedLoader;
using Sons.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SonsSdk;

public class GlobalInput
{
    public static MelonEvent OnUsePerformed = new();

    
    private static List<KeyRegistration> _keyRegistrations = new();

    private static InputAction _useAction;

    /// <summary>
    /// Register a key that will be polled every frame. The action will be invoke if the key is pressed during that frame.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="action"></param>
    /// <returns>False if the key is already registered</returns>
    public static bool RegisterKey(KeyCode key, Action action)
    {
        if(_keyRegistrations.Any(k => k.Key == key))
            return false;
        
        _keyRegistrations.Add(new KeyRegistration
        {
            Key = key,
            Action = action
        });

        return true;
    }
    
    public static void UnregisterKey(KeyCode key)
    {
        _keyRegistrations.RemoveAll(k => k.Key == key);
    }

    internal static void Init()
    {
        GlobalEvents.OnUpdate.Subscribe(OnUpdate);
        SdkEvents.OnGameActivated.Subscribe(OnGameActivated);
    }

    private static void OnGameActivated()
    {
        _useAction = Sons.Input.InputSystem.InputMapping.@default.Use;
    }

    private static void OnUpdate()
    {
        foreach (var keyRegistration in _keyRegistrations)
        {
            if (Input.GetKeyDown(keyRegistration.Key))
                keyRegistration.Action();
        }

        if (_useAction != null)
        {
            if (_useAction.WasPerformedThisFrame())
            {
                OnUsePerformed.Invoke();
            }
        }
    }

    private struct KeyRegistration
    {
        public KeyCode Key;
        public Action Action;
    }
}