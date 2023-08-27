using Sons.Ai.Vail;
using SonsSdk.Exceptions;
using UnityEngine;

namespace SonsSdk;

public static class ActorTools
{
    public static VailActor GetPrefab(VailActorTypeId id)
    {
        if (!VailWorldSimulation.TryGetInstance(out var inst))
            throw new NotInWorldException();
        
        return inst._actorPools.GetPrefab(id);
    }
    
    public static VailActor Spawn(VailActorTypeId id, Vector3 position, Quaternion rotation)
    {
        var prefab = GetPrefab(id);
        if (!prefab)
            return null;
        
        return prefab.gameObject.InstantiateAndGet<VailActor>(position, rotation);
    }

    public static IEnumerable<VailActor> GetActors(VailActorTypeId id)
    {
        if (!VailActorManager._instance)
            yield break;
        
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