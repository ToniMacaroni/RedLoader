# Creating Items

```csharp
var prefab = DebugTools.CreatePrimitive(PrimitiveType.Sphere);
prefab.transform.localScale = Vector3.one * 0.1f;

// register a new item
var newItem = ItemTools.CreateAndRegisterItem(8000, "FancyBone", maxAmount:10);

// add item locations on the inventory mat and the crafting mat
// also setup a crafting recipe
new ItemTools.ItemBuilder(prefab, newItem)
    .AddInventoryItem(
        new Vector3(-1.31599998f,0,2.21799994f),
        new Vector3(0,0,0.1f), 
        new Vector3(-0.1f,0,0)) // adds inventory layout
    .AddIngredientItem(new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) // adds ingredient layout
    .AddCraftingResultItem(new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) // adds crafting result layout
    
    .Recipe // register new recipe
    .AddIngredient(ItemTools.Identifiers.Stick, 2)
    .BuildAndAdd();

prefab.TryDestroy();
```