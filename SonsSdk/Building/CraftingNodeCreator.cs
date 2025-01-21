using System.Text.RegularExpressions;
using Bolt;
using Construction;
using Endnight.Localization;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using RedLoader;
using RedLoader.Utils;
using Sons.Crafting;
using Sons.Crafting.Structures;
using Sons.Weapon;
using SonsSdk.Networking;
using UnityEngine;
using UnityGLTF;
using Color = System.Drawing.Color;
using Object = UnityEngine.Object;

namespace SonsSdk.Building;

public record ScrewStructureRegistration(
    GameObject prefab,
    int recipeId,
    string recipeName,
    Action<StructureCraftingNode, GameObject> nodeAndBuiltProcessor = null);

public class CraftingNodeCreator
{
    internal static bool ServerMode => LoaderEnvironment.IsDedicatedServer;
    
    public static string NextLocalizationString = "BLUEPRINT_PAGE_MISC";
    public static StructureRecipe.CategoryType StructureCategory = StructureRecipe.CategoryType.Decoration;
    
    private static readonly Regex PascalToHumanRegex = new("[a-z][A-Z]", RegexOptions.Compiled);
    private static StructureCraftingNode _craftingNodePrefab;
    private static StructureRecipe _recipePrefab;
    private static readonly Dictionary<int, ScrewStructureRegistration> LoadedObjects = new();
    private static readonly Dictionary<int, StructureRecipe> ProcessedObjects = new();

    /// <summary>
    /// Make an empty crafting node setup from the lean to structure.
    /// </summary>
    private static void InitializePrefabs()
    {
        if (_craftingNodePrefab)
            return;
        
        var structure = ConstructionTools.GetRecipe(25)._structureNodePrefab;
        var go = structure.Instantiate();
        go.name = "CustomStructurePrefab";
        var structureCraftingNode = go.GetComponent<StructureCraftingNode>();
        go.transform.Find("Ingredients").gameObject.TryDestroy();
        go.DontDestroyOnLoad().SetActive(false);
        
        _craftingNodePrefab = structureCraftingNode;
        _recipePrefab = structureCraftingNode._recipe;
        var newRecipe = Object.Instantiate(structureCraftingNode._recipe);
        newRecipe._id = 666;
        structureCraftingNode._recipe = newRecipe;
    }

    private static void TryRegister(ScrewStructureRegistration reg)
    {
        if (!LoadedObjects.TryAdd(reg.recipeId, reg))
        {
            throw new Exception($"Recipe Collision!!! Blueprint with id {reg.recipeId} already registered!");
        }
    }

    public static IEnumerable<ScrewStructureRegistration> GetRegistrations() => LoadedObjects.Values;
    public static IEnumerable<StructureRecipe> GetProcessedRecipes() => ProcessedObjects.Values;
    
    internal static void OnGameActivated()
    {
        InitializePrefabs();

        foreach (var loadedObject in LoadedObjects.Values)
        {
            SetupObjectAsCraftingNode(loadedObject);
        }
    }

    internal static async void Init()
    {
        SdkEvents.OnSonsSceneInitialized.Subscribe(OnSceneInit);

        await LoadGltfObjectsFromDirectory();
    }

    internal static void Uninit()
    {
        
    }

    private static void OnSceneInit(ESonsScene scene)
    {
        if (scene != ESonsScene.Game) return;
        
        OnGameActivated();
    }

    public static void SetupRecipeIngredients(StructureRecipe recipe)
    {
        var node = recipe._structureNodePrefab.GetComponent<StructureCraftingNode>();
        var ingredients = recipe._ingredients = new Il2CppSystem.Collections.Generic.List<StructureCraftingRecipeIngredient>();
        foreach (var link in node._craftingIngredientLinks)
        {
            var ingredient = link.Ingredient;
            ingredients.Add(new(){ItemId = ingredient.ItemId, Count = ingredient.Count});
        }
    }

    /// <summary>
    /// Prepare the built variant of a structure by removing all crafting node specific things like <see cref="StructureCraftingNodeIngredient"/>s.
    /// Basically a conversion from blueprint to built (finished) structure.
    /// </summary>
    /// <param name="built"></param>
    /// <param name="recipe"></param>
    public static void PrepareBuiltStructure(GameObject built, StructureRecipe recipe)
    {
        foreach (var ingredient in built.GetComponentsInChildren<StructureCraftingNodeIngredient>(true))
        {
            Object.Destroy(ingredient);
        }
        
        if (!built.GetComponent<BoltEntity>())
        {
            var bolt = built.AddComponent<BoltEntity>().Init(recipe._id, BoltFactories.ScrewStructureState);
            bolt._queryOptionIEntityBehaviour = QueryOptions.GetComponentsInChildren;
        }
        
        EntityManager.RegisterPrefab(built);

        var screwStructure = built.AddComponent<ScrewStructure>();
        screwStructure._recipe = recipe;
        screwStructure.enabled = false;
        
        built.AddComponent<FreeFormStructureBuiltLinker>();
        built.AddComponent<ScrewStructureDestruction>();
        built.AddComponent<BoxCollider>();
        
        built.DontDestroyOnLoad();

        built.AsPrefab(screwStructure);
    }

    /// <summary>
    /// Tries to register a recipe with the recipe database.
    /// Checks if an instance of the <see cref="StructureCraftingSystem"/> exists and if the recipe isn't already registered.
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns>True if the recipe got registered successfully</returns>
    public static bool AddRecipeToDatabase(StructureRecipe recipe)
    {
        if (!StructureCraftingSystem.TryGetInstance(out var system))
        {
            return false;
        }

        if (system._recipeDatabase.TryGetRecipeById(recipe._id, out _))
        {
            return false;
        }
        
        system._recipeDatabase._recipes.Add(recipe);
        return true;
    }

    /// <summary>
    /// Creates and registers a new recipe.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="node"></param>
    /// <param name="built"></param>
    /// <returns></returns>
    public static StructureRecipe CreateNewRecipe(int id, string name, StructureCraftingNode node = null, GameObject built = null)
    {
        var recipe = Object.Instantiate(_recipePrefab);
        recipe._id = id;
        recipe._boltPrefabId = id - 1;
        recipe._category = StructureCategory;

        if (node)
        {
            recipe._structureNodePrefab = node.gameObject;
            node._recipe = recipe;
        }

        if (built)
            recipe._builtPrefab = built;

        recipe.name = name;
        recipe._displayName = ToDisplayName(name);
        recipe.hideFlags = HideFlags.HideAndDontSave;
        
        AddRecipeToDatabase(recipe);

        RLog.Msg(Color.LawnGreen, $"Added recipe {recipe.name}");
        
        return recipe;
    }

    /// <summary>
    /// Turn a transform containing <see cref="StructureElement"/>s into a ghost and add ingredient descriptors.
    /// </summary>
    /// <param name="freeformStructure"></param>
    public static void ProcessFreeformStructure(Transform freeformStructure)
    {
        foreach (var structureElement in freeformStructure.GetComponentsInChildren<StructureElement>())
        {
            foreach (var rend in structureElement.gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (rend.name is "TerrainBumpProjector" or "Shadows" or "GrassPusher")
                {
                    rend.gameObject.TryDestroy();
                    continue;
                }

                if (!rend.GetComponent<StructureGhostSwapper>())
                    rend.gameObject.AddComponent<StructureGhostSwapper>();

                //if (rend.TryGetComponent(out MeshOutliner outliner))
                //     {
                //         Object.Destroy(outliner);
                //     }
            }

            var itemId = structureElement._profile?._item?._id ?? 0;

            if (itemId < 1)
                continue;

            var ingredient = structureElement.gameObject.AddComponent<StructureCraftingNodeIngredient>();
            ingredient.OnValidate();
            ingredient._itemId = itemId;
            ingredient._requiresAllPreviousIngredients = false;
        }
    }

    /// <summary>
    /// Turn a transform containing <see cref="MeshRenderer"/>s into a ghost and add ingredient descriptors.
    /// Each renderer will be treated as a separate ingredient.
    /// </summary>
    /// <param name="structureRoot"></param>
    /// <param name="itemId">The item id for the ingredients.</param>
    public static List<StructureCraftingNodeIngredient> ProcessStructure(Transform structureRoot, int itemId)
    {
        var ingredients = new List<StructureCraftingNodeIngredient>();
        
        foreach (var rend in structureRoot.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.name is "TerrainBumpProjector" or "Shadows" or "GrassPusher")
            {
                rend.gameObject.TryDestroy();
                continue;
            }

            if (!rend.GetComponent<StructureGhostSwapper>())
                rend.gameObject.AddComponent<StructureGhostSwapper>();
            
            var ingredient = rend.gameObject.AddComponent<StructureCraftingNodeIngredient>();
            ingredient.OnValidate();
            ingredient._itemId = itemId;
            ingredient._requiresAllPreviousIngredients = false;
            ingredients.Add(ingredient);
        }

        return ingredients;
    }

    /// <summary>
    /// Get all <see cref="StructureCraftingNodeIngredient"/>s in a transform and its children.
    /// </summary>
    /// <param name="structureRoot"></param>
    public static Il2CppArrayBase<StructureCraftingNodeIngredient> GetIngredients(Transform structureRoot)
        => structureRoot.GetComponentsInChildren<StructureCraftingNodeIngredient>(true);

    /// <summary>
    /// Wraps <see cref="transformToWrap"/> with a crafting node setting up all ingredients and ghosts for the structure.
    /// <see cref="transformToWrap"/> needs to have <see cref="StructureElement"/>s as children.
    /// </summary>
    /// <param name="transformToWrap"></param>
    /// <param name="isFreeform">true for structures composed out of <see cref="StructureElement"/>s</param>
    /// <param name="ingredientItemId"></param>
    /// <returns></returns>
    public static StructureCraftingNode CreateCraftingNode(
        Transform transformToWrap,
        int? ingredientItemId = null)
    {
        var ingredients = 
            ingredientItemId == null 
            ? GetIngredients(transformToWrap).ToList() 
            : ProcessStructure(transformToWrap, ingredientItemId.Value);

        var structureCraftingNode = Object.Instantiate(_craftingNodePrefab);
        structureCraftingNode.name = transformToWrap.name.Replace("(Clone)", "") + "CraftingNode";

        foreach (var node in structureCraftingNode.GetComponentsInChildren<StructureCraftingNodeIngredient>(true))
        {
            node.gameObject.TryDestroy();
        }
        foreach (var structureElement in structureCraftingNode.GetComponentsInChildren<StructureElement>(true))
        {
            structureElement.gameObject.TryDestroy();
        }
        foreach (var collider in transformToWrap.GetComponentsInChildren<Collider>())
        {
            Object.Destroy(collider);
        }
        
        var go = structureCraftingNode.gameObject.DontDestroyOnLoad();
        structureCraftingNode.enabled = false;
        structureCraftingNode._isActive = false;
        go.AsPrefab(structureCraftingNode);
        var tr = go.transform;
        
        go.transform.Find("Ingredients")?.gameObject.TryDestroy();
        // go.transform.Find("AutoFoundationRope").gameObject.TryDestroy();
        // go.Destroy<AutoFoundations>();
        // go.Destroy<BoltEntity>();
        // go.Destroy<StructureDestruction>();
        // go.Destroy<FreeFormStructureLinker>();

        tr.SetPositionAndRotation(transformToWrap.position, transformToWrap.rotation);

        transformToWrap.SetParent(tr, false);
        transformToWrap.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var groupings = new Dictionary<int, Il2CppSystem.Collections.Generic.List<StructureCraftingNodeIngredient>>();
        foreach (var node in ingredients)
        {
            var itemId = node.ItemId;
            
            if (!groupings.ContainsKey(itemId))
                groupings.Add(itemId, new());
            
            groupings[itemId].Add(node);
        }
        
        structureCraftingNode._craftingIngredientLinks.Clear();
        foreach (var (itemId, nodesIngredients) in groupings)
        {
            structureCraftingNode._craftingIngredientLinks.Add(new()
            {
                Ingredient = new()
                {
                    Count = nodesIngredients.Count,
                    ItemId = itemId
                },
                _ingredients = nodesIngredients
            });
        }
        
        go.SetActive(true);
        
        
        return structureCraftingNode;
    }

    public static void InitObjectInteraction(StructureCraftingNode node)
    {
        if (node._ingredientUiTemplate)
            return;
        
        var newInteraction = _craftingNodePrefab.transform.Find("StructureInteractionObjects").gameObject.Instantiate();
        var t = newInteraction.transform;
        t.SetParent(node.transform, false);
        t.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        
        node._ingredientUiTemplate = t.Find("Canvas/UiRoot/Ingredients/IngredientUiTemplate").gameObject;
        node._cancelStructureInteractionElement = t.Find("Canvas/UiRoot/CancelStructureInteractionElement").gameObject;
        node._ingredientUi = new Il2CppSystem.Collections.Generic.List<StructureCraftingNodeIngredientUi>();
        node._ingredientUi.Add(t.Find("Canvas/UiRoot/Ingredients/IngredientUi").GetComponent<StructureCraftingNodeIngredientUi>());

        t.Find("UiLocator").localPosition = node.GetComponent<BoxCollider>().center;
    }
    
    public static void InitRecipeLocalization(StructureRecipe recipe)
    {
        var key = LocalizationUtils.GetLocalizationKey(recipe._displayName);
        LocalizationTools.BlueprintBookTable.AddEntry(key, recipe._displayName);
        recipe._localizationId = key;
    }

    /// <summary>
    /// Adds a recipe to the database and creates a node prefab if it doesn't exist.
    /// </summary>
    /// <param name="recipe"></param>
    private static void InitRecipe(StructureRecipe recipe)
    {
        AddRecipeToDatabase(recipe);

        EntityManager.RegisterPrefab(recipe._builtPrefab);

        if (recipe._structureNodePrefab)
        {
            EntityManager.RegisterPrefab(recipe._structureNodePrefab);
            InitObjectInteraction(recipe._structureNodePrefab.GetComponent<StructureCraftingNode>());
            return;
        }

        var builtCopy = recipe._builtPrefab.Instantiate();
        var craftingNode = CreateCraftingNode(builtCopy.transform);
        recipe._structureNodePrefab = craftingNode.gameObject;
    }

    private static void CreateBookPage(StructureRecipe topRecipe, StructureRecipe bottomRecipe, Texture2D background)
    {
        if(topRecipe)
        {
            InitRecipeLocalization(topRecipe);
            InitRecipe(topRecipe);
        }

        if(bottomRecipe)
        {
            InitRecipeLocalization(bottomRecipe);
            InitRecipe(bottomRecipe);
        }

        var data = new BlueprintBookPageData
        {
            _topRecipe = topRecipe,
            _bottomRecipe = bottomRecipe,
            _pageTitleLocalizationId = NextLocalizationString
        };

        var controller = ItemTools.GetHeldPrefab(ItemTools.Identifiers.BlueprintBook).GetComponent<BlueprintBookController>();
        data._pageImage = background;
        controller._pages._pages.Add(data);
    }
    
    /// <summary>
    /// Creates a crafting node, prepares a built prefab, sets up recipe ingredients, creates a recipe and registers it with the database.
    /// If the recipe already exists (the object has already been processes) this method ONLY registers the recipe without processing anything.
    /// </summary>
    /// <param name="reg"></param>
    public static void SetupObjectAsCraftingNode(ScrewStructureRegistration reg)
    {
        if (ProcessedObjects.TryGetValue(reg.recipeId, out var existingRecipe))
        {
            AddRecipeToDatabase(existingRecipe);
            return;
        }
        
        // create crafting node
        var craftingNode = CreateCraftingNode(reg.prefab.Instantiate(Vector3.zero, Quaternion.identity).transform);
        var be = craftingNode.GetComponent<BoltEntity>();
        be._prefabId = reg.recipeId - 1;
        EntityManager.RegisterPrefab(be);
        
        // create recipe
        var built = reg.prefab.Instantiate(Vector3.zero, Quaternion.identity);
        var recipe = CreateNewRecipe(reg.recipeId, reg.recipeName, craftingNode, built);
        PrepareBuiltStructure(built, recipe);
        SetupRecipeIngredients(recipe);
        
        reg.nodeAndBuiltProcessor?.Invoke(craftingNode, built);

        ProcessedObjects[recipe._id] = recipe;

        RLog.Msg(Color.PaleGreen, $"Registered gltf crafting node with recipe id {recipe._id}");
    }
    
    public static async Task LoadGltfObjectsFromDirectory(string directory = null)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = LoaderEnvironment.BlueprintDirectory;
        }

        foreach (var file in Directory.GetFiles(directory))
        {
            if(!file.EndsWith(".gltf") && !file.EndsWith(".glb"))
                continue;
            
            var fileName = Path.GetFileNameWithoutExtension(file);
            var recipeId = GetRecipeIdFromName(fileName);

            if (fileName.Contains(" "))
            {
                RLog.Error($"Name of blueprint cannot contain spaces ({file})");
                continue;
            }

            if (recipeId == null)
            {
                RLog.Error("Couldn't setup recipe id from name");
                continue;
            }

            var loader = new GLTFSceneImporter(file, new());
            loader.ProcessMaterials = !ServerMode;
            await loader.LoadSceneAsync();

            TryRegister(new(loader.CreatedObject.HideAndDontSave(), recipeId.Value, fileName + "Recipe"));
            
            RLog.Msg(Color.PaleGreen, $"Loaded {file}");
        }
    }

    public static int? GetRecipeIdFromName(string name)
    {
        if (!name.Contains('.'))
            return LoaderUtils.HashString(name);

        var split = name.Split('.');
        if (!int.TryParse(split[^1], out var id))
            return LoaderUtils.HashString(name);

        return id;
    }
    
    public static string ToDisplayName(string str)
    {
        str = str.Replace("Recipe", "");
        str = str.Split('.')[0];
        return PascalToHumanRegex.Replace(str, m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
    }
}
