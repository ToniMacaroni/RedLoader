using Il2CppInterop.Runtime;
using RedLoader.Utils;
using Sons.Items.Core;
using UnityEngine;

namespace SonsSdk;

public class ItemTools
{
    internal static readonly BufferedAdder<ItemHook> ItemHookAdder = new(() => ItemDatabaseManager._instance, hook =>
    {
        var item = ItemDatabaseManager.ItemById(hook.ItemId);
        if (item)
        {
            if (hook.HookType.HasFlag(EPrefabType.HeldPrefab) && item.HeldPrefab)
            {
                item.HeldPrefab.gameObject.AddComponent(hook.ComponentType);
            }

            if (hook.HookType.HasFlag(EPrefabType.PickupPrefab) && item.PickupPrefab)
            {
                item.PickupPrefab.gameObject.AddComponent(hook.ComponentType);
            }
        }
    });
    
    public static void AddItemComponent<T>(int itemId, EPrefabType hookType)
    {
        ItemHookAdder.Add(new ItemHook(itemId, typeof(T), hookType));
    }
    
    public static void AddItemComponent(int itemId, Type component, EPrefabType hookType)
    {
        ItemHookAdder.Add(new ItemHook(itemId, component, hookType));
    }

    public struct ItemHook
    {
        public readonly int ItemId;
        public readonly EPrefabType HookType;

        internal readonly Il2CppSystem.Type ComponentType;

        public ItemHook(int itemId, Type component, EPrefabType hookType)
        {
            ItemId = itemId;
            HookType = hookType;
            ComponentType = Il2CppType.From(component);
        }
    }
    
    [Flags]
    public enum EPrefabType
    {
        HeldPrefab = 0,
        PickupPrefab = 1 << 0,
        PropPrefab = 1 << 1,
    }
}