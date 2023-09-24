using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Pathfinding;
using Sons.Ai.Vail;
using Sons.Areas;
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

    public static Il2CppArrayBase<VailActorPools.PoolTypeList> GetPrefabs()
    {
        if (!VailWorldSimulation.TryGetInstance(out var inst))
            throw new NotInWorldException();

        return inst._actorPools._poolsByType._items;
    }
    
    public static VailActor Spawn(VailActorTypeId id, Vector3 position, int variationId = 0)
    {
        var prefab = GetPrefab(id);
        if (!prefab)
            return null;

        if (!VailWorldSimulation.TryGetInstance(out var vailWorldSimulation))
            throw new NotInWorldException();
        
        WorldSimActor worldSimActor = vailWorldSimulation.SpawnActor(
            prefab, 
            position, 
            GraphMask.everything, 
            null, 
            State.None, 
            VailWorldSimulation.NewFamily(), 
            variationId);
        
        if (worldSimActor == null)
        {
            return null;
        }

        worldSimActor.SetKeepAboveTerrain(true);
        var currentAreaMask = CaveEntranceManager.CurrentAreaMask;
        worldSimActor.SetAreaMask(currentAreaMask);
        worldSimActor.SetGraphMask(vailWorldSimulation.GetNavGraphMaskForArea(currentAreaMask));
        return vailWorldSimulation.ConvertToRealActor(worldSimActor, prefab);
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
    
    public static VailActor GetClosestActor(Vector3 position)
    {
        VailActor closest = null;
        var closestDistance = float.MaxValue;
        foreach (var actor in VailActorManager._instance._activeActors)
        {
            var distance = Vector3.Distance(actor.transform.position, position);
            if (distance < closestDistance)
            {
                closest = actor;
                closestDistance = distance;
            }
        }

        return closest;
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