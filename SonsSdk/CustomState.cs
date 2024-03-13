using RedLoader;
using Sons.Gui;
using Sons.Input;
using UnityEngine.InputSystem;
using InputSystem = UnityEngine.InputSystem.InputSystem;

namespace SonsSdk;

/// <summary>
/// Class to create custom states i.e. temporarily disable (override) some inputs and have some update function.
/// </summary>
public abstract class CustomState
{
    public bool IsActive { get; private set; }

    protected InputAction[] ActionsToDisable;
    protected string[] UisToToggle;
    protected InputAction[] CustomActions;
    
    private SonsInputMapping.DefaultActions DefaultInputs => Sons.Input.InputSystem._sonsInputMapping.@default;
    
    protected CustomState() {}

    protected CustomState(InputAction[] actionsToDisable)
    {
        ActionsToDisable = actionsToDisable;
    }
    
    public void Start()
    {
        if(ActionsToDisable != null)
            foreach (var action in ActionsToDisable)
                action.Disable();
        
        if(UisToToggle != null)
            foreach (var ui in UisToToggle)
                UiManager.SetActive(ui, true);
        
        if(CustomActions != null)
            foreach (var action in CustomActions)
                action.Enable();

        OnStart();
        GlobalEvents.OnUpdate.Subscribe(OnUpdateInternal);
        IsActive = true;
    }

    public void End()
    {
        IsActive = false;
        GlobalEvents.OnUpdate.Subscribe(OnUpdateInternal);
        OnEnd();
        
        if(ActionsToDisable != null)
            foreach (var action in ActionsToDisable)
                action.Enable();
        
        if(UisToToggle != null)
            foreach (var ui in UisToToggle)
                UiManager.SetActive(ui, false);
        
        if(CustomActions != null)
            foreach (var action in CustomActions)
                action.Disable();
    }

    private void OnUpdateInternal()
    {
        if(!IsActive)
            return;
        
        OnUpdate();
    }
    
    protected abstract void OnStart();

    protected abstract void OnEnd();

    protected abstract void OnUpdate();
}