using RedLoader;
using UnityEngine.InputSystem;

namespace SonsSdk;

public abstract class CustomState
{
    public bool IsActive { get; private set; }

    protected InputAction[] ActionsToDisable;
    
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