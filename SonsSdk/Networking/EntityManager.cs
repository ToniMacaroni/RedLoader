using Bolt;
using HarmonyLib;
using RedLoader;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SonsSdk.Networking;

public static class EntityManager
{
    private static readonly List<BoltEntity> RegisteredEntities = new();

    internal static void Init()
    {
        SdkEntryPoint.Harmony.Patch(
            AccessTools.Method(typeof(PrefabDatabase), nameof(PrefabDatabase.UpdateLookup)),
            postfix: new(typeof(EntityManager), nameof(OnUpdateLookup)));
    }

    internal static void Uninit()
    {
        RegisteredEntities.Clear();
    }
    
    private static void OnUpdateLookup()
    {
        var added = new List<GameObject>();
        
        foreach (var entity in RegisteredEntities)
        {
            var go = entity.gameObject;
            
            if (PrefabDatabase.Instance.Prefabs.Contains(go))
            {
                return;
            }

            added.Add(go);
            PrefabDatabase._lookup[entity.prefabId] = go;
        }

        //PrefabDatabase.Instance.Prefabs = PrefabDatabase.Instance.Prefabs.Concat(added).ToArray();

        RLog.Msg(Color.DeepPink, "initialized bolt prefabs");
    }
    
    /// <summary>
    /// Register a BoltEntity to Bolt.
    /// Optionally specify a serializer if the BoltEntity doesn't already have that filled in (See <see cref="BoltFactories"/>).
    /// </summary>
    /// <param name="entity">The BoltEntity to register</param>
    /// <param name="serializer">The optional serializer. If you don't use the overload here, make sure to assign it somewhere else before</param>
    public static void RegisterPrefab(BoltEntity entity, string serializer = null)
    {
        if(RegisteredEntities.Contains(entity))
            return;

        if (!string.IsNullOrEmpty(serializer))
        {
            entity._serializerGuid = serializer;
        }
        
        if (BoltNetwork.isRunning)
        {
            PrefabDatabase._lookup[entity.prefabId] = entity.gameObject;
        }
        
        RegisteredEntities.Add(entity);
    }

    /// <summary>
    /// Register a BoltEntity to Bolt by getting the component from a GameObject.
    /// Optionally specify a serializer if the BoltEntity doesn't already have that filled in (See <see cref="BoltFactories"/>).
    /// </summary>
    /// <param name="go">The GameObject with the BoltEntity on it.</param>
    /// <param name="serializer">The optional serializer. If you don't use the overload here, make sure to assign it somewhere else before</param>
    public static void RegisterPrefab(GameObject go, string serializer = null)
    {
        if (go.TryGetComponent(out BoltEntity entity))
            RegisterPrefab(entity, serializer);
    }
    
    public static void UnregisterPrefab(BoltEntity entity)
    {
        RegisteredEntities.Remove(entity);
    }
}
