using Bolt;
using HarmonyLib;
using RedLoader.Utils;
using UnityEngine;

namespace SonsSdk;

public static class Networking
{
    public static bool IsMultiplayer => BoltNetwork.isRunning;
    public static bool IsServer => BoltNetwork.isServer;
    public static bool IsClient => BoltNetwork.isClient;
    public static bool IsDedicatedServer => LoaderEnvironment.IsDedicatedServer;
    
    private static readonly List<BoltEntity> _registerdEntities = new();

    internal static void Init()
    {
        SdkEvents.OnGameActivated.Subscribe(OnGameActivated);
    }

    internal static void Uninit()
    {
        SdkEvents.OnGameActivated.Unsubscribe(OnGameActivated);
        _registerdEntities.Clear();
    }

    private static void OnGameActivated()
    {
        if (!BoltNetwork.isRunning)
            return;

        var added = new List<GameObject>();
        
        foreach (var entity in _registerdEntities)
        {
            var go = entity.gameObject;
            
            if (PrefabDatabase.Instance.Prefabs.Contains(go))
            {
                return;
            }

            added.Add(go);
            PrefabDatabase._lookup[entity.prefabId] = go;
        }

        PrefabDatabase.Instance.Prefabs = PrefabDatabase.Instance.Prefabs.Concat(added).ToArray();
    }

    public static void RegisterPrefab(BoltEntity entity)
    {
        if(_registerdEntities.Contains(entity))
            return;
        
        _registerdEntities.Add(entity);
    }
    
    public static void RegisterPrefab(GameObject go)
    {
        if (go.TryGetComponent(out BoltEntity entity))
            RegisterPrefab(entity);
    }
    
    public static void UnregisterPrefab(BoltEntity entity)
    {
        _registerdEntities.Remove(entity);
    }
}