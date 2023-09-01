using RedLoader;
using Sons.Ai.Vail;
using SonsSdk;
using TheForest.Utils;
using UnityEngine;

namespace SonsGameManager;

public class DebugGizmoTest
{
    private DebugTools.LineDrawer _drawer;
    private VailActor _robby;
    
    public DebugGizmoTest()
    {
        GlobalInput.RegisterKey(KeyCode.F6, () =>
        {
            _robby = ActorTools.GetRobby();
            _drawer = new DebugTools.LineDrawer();
            RLog.Debug("Started line debugging");
        });
        
        SdkEvents.OnInWorldUpdate.Subscribe(OnInWorldUpdate);
    }
    
    private void OnInWorldUpdate()
    {
        if(!_robby || _drawer == null)
            return;

        var t = _robby.transform;
        var position = t.position;
        _drawer.SetLine(position, position + t.forward * 10);
    }
}