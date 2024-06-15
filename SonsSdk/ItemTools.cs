using Endnight.Utilities;
using Il2CppInterop.Runtime;
using RedLoader.Utils;
using Sons.Crafting;
using Sons.Inventory;
using Sons.Items.Core;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;
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
    
    public static InventoryLayoutItemGroup GetInventoryLayoutItemGroup(int itemId)
    {
        return LocalPlayer.Inventory.GetComponentsInChildren<InventoryLayoutItemGroup>(true).FirstOrDefault(x => x._itemId == itemId);
    }
    
    public static IngredientLayoutItemGroup GetIngredientLayoutItemGroup(int itemId)
    {
        return GameState.CraftingSystem._ingredientLayoutGroups.GetComponentsInChildren<IngredientLayoutItemGroup>(true).FirstOrDefault(x => x._itemId == itemId);
    }
    
    public static CraftingResultLayoutItemGroup GetCraftingResultLayoutItemGroup(int itemId)
    {
        return GameState.CraftingSystem._craftingResultLayoutGroups.GetComponentsInChildren<CraftingResultLayoutItemGroup>(true).FirstOrDefault(x => x._itemId == itemId);
    }

    /// <summary>
    /// Replace a model in the inventory with a new prefab.
    /// </summary>
    /// <param name="prefab">The prefab that gets instantiated for each layout item.</param>
    /// <param name="itemId">The item id of the item to replace.</param>
    /// <param name="instantiated">A list of the objects that got instantiated.</param>
    /// <exception cref="Exception"></exception>
    public static void InitInventoryModelReplacement(GameObject prefab, int itemId, out List<GameObject> instantiated)
    {
        var layoutItemGroup = GetInventoryLayoutItemGroup(itemId);
        instantiated = new();
        
        if (!layoutItemGroup)
            throw new Exception($"InventoryLayoutItemGroup with itemId {itemId} not found.");
        
        var layer = LayerMask.NameToLayer("Inventory");

        foreach (var layoutItem in layoutItemGroup._layoutItems)
        {
            var itemRenderable = layoutItem.GetComponentInChildren<ItemRenderable>(true).transform;
            UnityEngine.Object.Destroy(itemRenderable.GetComponent<ItemRenderable>());
            var newModel = prefab.Instantiate();
            var tr = newModel.transform;
            tr.SetParent(itemRenderable);
            tr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            newModel.layer = layer;
            var proxy = newModel.AddComponent<MouseEventsProxy>();
            proxy._mouseEnterEvent.AddListener((UnityAction)layoutItem.OnMouseEnter);
            proxy._mouseOverEvent.AddListener((UnityAction)layoutItem.OnMouseOver);
            proxy._mouseExitEvent.AddListener((UnityAction)layoutItem.OnMouseExit);
            
            instantiated.Add(newModel);
        }
    }
    
    /// <summary>
    /// Create a new item and register it in the item database.
    /// </summary>
    public static ItemData CreateAndRegisterItem(
        int itemId, 
        string itemName, 
        int maxAmount = 10, 
        Texture2D icon = null, 
        string description = null)
    {
        var newData = UnityEngine.Object.Instantiate(ItemDatabaseManager.ItemById(ItemTools.Identifiers.Feather));
        newData._id = itemId;
        newData._name = itemName;
        newData._maxAmount = maxAmount;
        newData._editorName = itemName;
        newData._uiData._title = itemName;
        newData._uiData._translationKey = null;
        if (icon)
            newData._uiData._icon = icon;
        if (!string.IsNullOrEmpty(description))
            newData._uiData._description = description;
        RegisterItem(newData);

        return newData;
    }

    public class ItemBuilder
    {
        public RecipeBuilder Recipe => _recipeBuilder ??= new RecipeBuilder().AddResult(_item._id);
        
        private RecipeBuilder _recipeBuilder;
        private GameObject _prefab;
        private ItemData _item;
        private string _groupName;
        private bool _isOneToOne;

        /// <summary>
        /// </summary>
        /// <param name="prefab">The prefab to instantiate for each item</param>
        /// <param name="item">The item for which to build content</param>
        /// <param name="isOneToOne">If one visual element = 1 item</param>
        public ItemBuilder(GameObject prefab, ItemData item, bool isOneToOne = false)
        {
            _prefab = prefab;
            _item = item;
            _isOneToOne = isOneToOne;
        }

        public ItemBuilder(GameObject prefab, int itemId, bool isOneToOne)
        {
            _prefab = prefab;
            _item = ItemDatabaseManager.ItemById(itemId);
            _isOneToOne = isOneToOne;
        }

        public ItemBuilder SetPrefab(GameObject prefab)
        {
            _prefab = prefab;

            return this;
        }

        public ItemBuilder SetItem(ItemData item)
        {
            _item = item;

            return this;
        }

        public ItemBuilder SetItem(int itemId)
        {
            _item = ItemDatabaseManager.ItemById(itemId);

            return this;
        }
        
        public ItemBuilder SetGroupName(string groupName)
        {
            _groupName = groupName;

            return this;
        }

        /// <summary>
        /// Adds an inventory layout to the inventory for the item.
        /// </summary>
        /// <param name="positions">first position is the group position (relative to inventory), every other position is and additional layout item relative to the group.</param>
        /// <returns></returns>
        public ItemBuilder AddInventoryItem(params Vector3[] positions)
        {
            var newOne = ItemTools.GetInventoryLayoutItemGroup(ItemTools.Identifiers.CatFood).gameObject.Instantiate(true);
            newOne.name = _groupName ?? $"{_item._name}LayoutGroup";
            var group = newOne.GetComponent<InventoryLayoutItemGroup>();
            group._itemId = _item._id;
            group._startDisabled = false;
            group._isOneToOneCollection = _isOneToOne;
            UnityEngine.Object.Destroy(group.GetComponent<InventoryEdibleItem>());
            
            void AddItemToGroup(bool reuse, Vector3? position = null)
            {
                var layoutItem = reuse ? group._layoutItems._items[0] : group._layoutItems._items[0].gameObject.Instantiate(true).GetComponent<InventoryLayoutItem>();;
                if(position.HasValue)
                    layoutItem.transform.localPosition = position.Value;
                var renderable = layoutItem.transform.Find("ItemRenderable");
                UnityEngine.Object.Destroy(renderable.GetComponent<ItemRenderable>());
                var newModel = _prefab.Instantiate();
                var newModelTr = newModel.transform;
                newModelTr.SetParent(renderable);
                newModelTr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                //layoutItem.RefreshInteractionComponents();

                if(!reuse)
                    group._layoutItems.Add(layoutItem);
            }

            if (positions.Length == 0)
            {
                AddItemToGroup(true);
            }
            else
            {
                for (var i = 0; i < positions.Length; i++)
                {
                    var position = positions[i];
                    AddItemToGroup(i == 0, i == 0 ? null : position);
                    
                    if (i == 0)
                    {
                        group.transform.localPosition = position;
                    }
                }
            }

            group.gameObject.SetActive(true);

            return this;
        }

        /// <summary>
        /// Adds an ingredient layout to the inventory for the item.
        /// Make sure there exist at least on recipe with that item as an ingredient, otherwise the game bugs out.
        /// </summary>
        /// <param name="positions">first position is the group position (relative to the crafting mat), every other position is and additional layout item relative to the group.</param>
        /// <returns></returns>
        public ItemBuilder AddIngredientItem(params Vector3[] positions)
        {
            var newOne = GetIngredientLayoutItemGroup(Identifiers.CannedFood).gameObject.Instantiate(true);
            newOne.name = _groupName ?? $"{_item._name}LayoutGroup";
            var group = newOne.GetComponent<IngredientLayoutItemGroup>();
            group._itemId = _item._id;
            group._isOneToOneCollection = _isOneToOne;

            void AddItemToGroup(bool reuse, Vector3? position = null)
            {
                var layoutItem = reuse ? group._layoutItems._items[0] : group._layoutItems._items[0].gameObject.Instantiate(true).GetComponent<IngredientLayoutItem>();;
                if(position.HasValue)
                    layoutItem.transform.localPosition = position.Value;
                UnityEngine.Object.Destroy(layoutItem.GetComponent<ItemRenderable>());
                var newModel = _prefab.Instantiate();
                var newModelTr = newModel.transform;
                newModelTr.SetParent(layoutItem.transform);
                newModelTr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                //layoutItem.RefreshInteractionComponents();
                
                if(!reuse)
                    group._layoutItems.Add(layoutItem);
            }

            if (positions.Length == 0)
            {
                AddItemToGroup(true);
            }
            else
            {
                for (var i = 0; i < positions.Length; i++)
                {
                    var position = positions[i];
                    AddItemToGroup(i == 0, i == 0 ? null : position);
                    
                    if (i == 0)
                    {
                        group.transform.localPosition = position;
                    }
                }
            }

            group.gameObject.SetActive(true);

            return this;
        }
        
        /// <summary>
        /// Adds an crafting result layout to the inventory for the item.
        /// </summary>
        /// <param name="positions">first position is the group position (relative to the crafting mat), every other position is and additional layout item relative to the group.</param>
        /// <returns></returns>
        public ItemBuilder AddCraftingResultItem(params Vector3[] positions)
        {
            var newOne = GetCraftingResultLayoutItemGroup(Identifiers.CatFood).gameObject.Instantiate(true);
            newOne.name = _groupName ?? $"{_item._name}LayoutGroup";
            var group = newOne.GetComponent<CraftingResultLayoutItemGroup>();
            group._itemId = _item._id;
            group._isOneToOneCollection = _isOneToOne;
            UnityEngine.Object.Destroy(group.GetComponent<InventoryEdibleItem>());

            void AddItemToGroup(bool reuse, Vector3? position = null)
            {
                var layoutItem = reuse ? group._layoutItems._items[0] : group._layoutItems._items[0].gameObject.Instantiate(true).GetComponent<CraftingResultLayoutItem>();;
                if(position.HasValue)
                    layoutItem.transform.localPosition = position.Value;
                var tag = layoutItem.GetComponent<ItemLocatorTag>();
                tag._itemId = _item._id;
                tag._name = _item._name;
                UnityEngine.Object.Destroy(layoutItem.GetComponent<ItemRenderable>());
                var newModel = _prefab.Instantiate();
                var newModelTr = newModel.transform;
                newModelTr.SetParent(layoutItem.transform);
                newModelTr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                //layoutItem.RefreshInteractionComponents();
                
                if(!reuse)
                    group._layoutItems.Add(layoutItem);
            }

            if (positions.Length == 0)
            {
                AddItemToGroup(true);
            }
            else
            {
                for (var i = 0; i < positions.Length; i++)
                {
                    var position = positions[i];
                    AddItemToGroup(i == 0, i == 0 ? null : position);
                    
                    if (i == 0)
                    {
                        group.transform.localPosition = position;
                    }
                }
            }

            group.gameObject.SetActive(true);

            return this;
        }
    }

    public class RecipeBuilder
    {
        private Il2CppSystem.Collections.Generic.List<CraftingIngredient> _ingredients = new();
        private Il2CppSystem.Collections.Generic.List<CraftingRecipe.ResultingItem> _results = new();

        public RecipeBuilder AddIngredient(int itemId, int count, bool isReusable = false)
        {
            _ingredients.Add(new()
            {
                ItemId = itemId,
                Count = count,
                IsReusable = isReusable
            });

            return this;
        }
        
        public RecipeBuilder AddResult(int itemId)
        {
            _results.Add(new()
            {
                Id = itemId,
            });

            return this;
        }

        /// <summary>
        /// Builds a recipe with the given ingredients and results.
        /// If you want to add the recipe to the recipe database, use <see cref="BuildAndAdd"/>
        /// </summary>
        /// <returns>The built recipe</returns>
        public CraftingRecipe BuildRecipe()
        {
            var newRecipe = UnityEngine.Object.Instantiate(GameState.CraftingSystem._recipeDatabase._recipes._items[0]);
            newRecipe._ingredients = _ingredients;
            newRecipe._resultingItems = _results;
            newRecipe._recipeType = CraftingRecipe.Type.CraftNewItem;
            newRecipe._weaponMod = null;
            newRecipe._useContainerDataForResultingItems = false;
            newRecipe._containerItemData = null;
            return newRecipe;
        }

        /// <summary>
        /// Builds a recipe with the given ingredients and results and adds it to the recipe database.
        /// </summary>
        /// <returns>The built recipe</returns>
        public CraftingRecipe BuildAndAdd()
        {
            var newRecipe = BuildRecipe();
            GameState.CraftingSystem._recipeDatabase._recipes.Add(newRecipe);
            return newRecipe;
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