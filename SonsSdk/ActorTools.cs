using Sons.Ai.Vail;
using UnityEngine;

namespace SonsSdk;

public static class ActorTools
{
    public static VailActor GetPrefab(VailActorTypeId id)
    {
        return VailWorldSimulation._instance._actorPools.GetPrefab(id);
    }
    
    public static VailActor Spawn(VailActorTypeId id, Vector3 position, Quaternion rotation)
    {
        return GetPrefab(id).gameObject.InstantiateAndGet<VailActor>(position, rotation);
    }

    public static IEnumerable<VailActor> GetActors(VailActorTypeId id)
    {
        foreach (var actor in VailActorManager._instance._activeActors)
        {
            if (actor._id == id)
                yield return actor;
        }
    }

    public static VailActor GetRobby()
    {
        return GetActors(VailActorTypeId.Robby).FirstOrDefault();
    }

    public static VailActor GetVailActor(this WorldSimActor worldSimActor)
    {
        return VailActorManager.FindActiveActor(worldSimActor);
    }
}