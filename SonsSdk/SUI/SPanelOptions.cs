using RedLoader;
using SonsSdk;
using UnityEngine;

namespace SUI;

public class SPanelOptions : SContainerOptions
{
    public string Id { get; internal set; }

    private bool _closeOnKeyRelease;
    
    public SPanelOptions(GameObject root) : base(root)
    {
    }
    
    public SPanelOptions BindKeyConfig(KeybindConfigEntry keybind, bool closeOnKeyRelease = false)
    {
        _closeOnKeyRelease = closeOnKeyRelease;
        keybind.Notify(KeyPressed, KeyReleased);
        return this;
    }

    private void KeyPressed()
    {
        if (_closeOnKeyRelease)
        {
            Active(true);
            return;
        }

        Toggle();
    }

    private void KeyReleased()
    {
        if(_closeOnKeyRelease)
            Active(false);
    }
}