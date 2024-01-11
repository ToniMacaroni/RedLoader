using RedLoader;
using RedLoader.Preferences;
using UnityEngine.InputSystem;

namespace SonsSdk;

public static class ModInputCache
{
    internal static readonly Dictionary<KeybindConfigEntry, ModKeybind> Keybinds = new();

    public static ModKeybind GetKeybind(this KeybindConfigEntry config)
    {
        if (Keybinds.TryGetValue(config, out var action))
        {
            return action;
        }

        Keybinds[config] = action = new ModKeybind(config);
        RLog.Msg("Registered keybind: " + config.Identifier + "::" + config.FullPath);
        action.Enable();
        return action;
    }
    
    public static InputAction GetAction(this KeybindConfigEntry config)
    {
        return GetKeybind(config).Action;
    }
    
    /// <summary>
    /// Registers a callback for the "Performed" event of the input action.
    /// </summary>
    public static void Notify(this KeybindConfigEntry config, Action performAction = null, Action releasedAction = null)
    {
        var keybind = GetKeybind(config);
        if (keybind == null)
        {
            return;
        }
        
        keybind.Notify(performAction, releasedAction);
    }

    /// <summary>
    /// Removes a previously registered callback for the "Performed" event of the input action.
    /// </summary>
    public static void RemoveNotify(this KeybindConfigEntry config, Action performAction = null, Action releasedAction = null)
    {
        var keybind = GetKeybind(config);
        if (keybind == null)
        {
            return;
        }
        
        keybind.RemoveNotify(performAction, releasedAction);
    }

    internal static void CheckAll()
    {
        foreach (var keybind in Keybinds.Values)
        {
            keybind.Check();
        }
    }
}

public class ModKeybind
{
    public readonly InputAction Action;
    public readonly KeybindConfigEntry ConfigEntry;
    
    private event Action OnPerformed;
    private event Action OnReleased;

    public ModKeybind(InputAction action, KeybindConfigEntry configEntry)
    {
        Action = action;
        ConfigEntry = configEntry;
    }
    
    public ModKeybind(KeybindConfigEntry configEntry)
    {
        ConfigEntry = configEntry;

        Action = new InputAction(configEntry.Identifier, binding: configEntry.FullPath);
    }

    /// <summary>
    /// Registers a callback for the "Performed" event of the input action.
    /// </summary>
    /// <param name="performedAction"></param>
    /// <param name="releasedAction"></param>
    public void Notify(Action performedAction = null, Action releasedAction = null)
    {
        if (performedAction != null)
        {
            OnPerformed += performedAction;
        }
        
        if (releasedAction != null)
        {
            OnReleased += releasedAction;
        }
    }

    /// <summary>
    /// Removes a previously registered callback for the "Performed" event of the input action.
    /// </summary>
    /// <param name="performedAction"></param>
    public void RemoveNotify(Action performedAction = null, Action releasedAction = null)
    {
        if (performedAction != null)
        {
            OnPerformed -= performedAction;
        }
        
        if (releasedAction != null)
        {
            OnReleased -= releasedAction;
        }
    }

    public void RemoveAllCallbacks()
    {
        OnPerformed = null;
    }

    public void Enable()
    {
        Action.Enable();
    }
    
    public void Disable()
    {
        Action.Disable();
    }

    public void SaveToConfig()
    {
        if (Action.controls.Count == 0)
            return;
        
        ConfigEntry.Value = Action.controls[0].name;
    }
    
    public void RevertToDefault()
    {
        ConfigEntry.ResetToDefault();
        Action.ApplyBindingOverride(ConfigEntry.FullPath);
    }

    internal void Check()
    {
        if (Action.triggered)
        {
            OnPerformed?.Invoke();
        }

        if (Action.WasReleasedThisFrame())
        {
            OnReleased?.Invoke();
        }
    }
}