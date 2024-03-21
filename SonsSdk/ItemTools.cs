using Il2CppInterop.Runtime;
using RedLoader.Utils;
using Sons.Items.Core;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace SonsSdk;

public partial class ItemTools
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
    
    public static Transform GetHeldPrefab(int itemId)
    {
        var item = ItemDatabaseManager.ItemById(itemId);
        if (!item)
            return null;

        return item._heldPrefab;
    }
    
    public static Transform GetPickupPrefab(int itemId)
    {
        var item = ItemDatabaseManager.ItemById(itemId);
        if (!item)
            return null;

        return item._pickupPrefab;
    }
    
    public static Transform GetPropPrefab(int itemId)
    {
        var item = ItemDatabaseManager.ItemById(itemId);
        if (!item)
            return null;

        return item._propPrefab;
    }
    
    public static (Texture icon, Texture outline) GetIcon(int itemId)
    {
        var item = ItemDatabaseManager.ItemById(itemId);
        if (!item)
            return default;

        return (item._uiData._icon, item._uiData._outlineIcon);
    }

    public static void RegisterItem(ItemData itemData, bool autoTranslationEntry = true)
    {
        if (ItemDatabaseManager._itemsCache.ContainsKey(itemData._id))
            throw new Exception($"Item with id {itemData._id} already registered!");
        
        ItemDatabaseManager._itemsCache.Add(itemData._id, itemData);
        ItemDatabaseManager._instance._itemDataList.Add(itemData);

        if (itemData._uiData != null && autoTranslationEntry)
        {
            SetupLocalizationForItem(itemData._uiData, itemData._uiData._title);
        }
    }
    
    private static void SetupLocalizationForItem(ItemUiData itemUiData, string itemTitle)
    {
        var table = LocalizationTools.ItemsTable;
        if (string.IsNullOrEmpty(itemUiData._translationKey))
        {
            itemUiData._translationKey = $"I_{itemUiData._itemId}";
        }

        if (itemUiData._applyCustomActionText)
        {
            var leftActionText = itemUiData._leftActionCustomText;
            var rightActionText = itemUiData._rightActionCustomText;

            if (!string.IsNullOrEmpty(leftActionText))
            {
                var key = $"{itemUiData._translationKey}_LEFT_ACTION";
                table.AddEntry(key, leftActionText);
                itemUiData._leftActionCustomText = key;
            }

            if (!string.IsNullOrEmpty(rightActionText))
            {
                var key = $"{itemUiData._translationKey}_RIGHT_ACTION";
                table.AddEntry(key, rightActionText);
                itemUiData._rightActionCustomText = key;
            }
        }

        var description = itemUiData._description;
        if (!string.IsNullOrEmpty(description))
        {
            table.AddEntry($"{itemUiData._translationKey}_DESC", description);
        }

        if (!string.IsNullOrEmpty(itemTitle))
        {
            table.AddEntry(itemUiData._translationKey, itemTitle);
            table.AddEntry($"{itemUiData._translationKey}_PLURAL", itemTitle + "s");
        }
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