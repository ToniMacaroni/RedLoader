using Construction;
using Sons.Crafting;
using TheForest.Utils;

namespace SonsSdk;

public static class GameState
{
    public static ConstructionManager ConstructionManager => RepositioningUtils.Manager;
    
    public static CraftingSystem CraftingSystem
    {
        get
        {
            if (!_craftingSystem)
            {
                _craftingSystem = LocalPlayer.Transform.FindGet<CraftingSystem>("InventorySystem/CraftingSystem");
            }

            return _craftingSystem;
        }
    }
    
    public static uint LastLoadedSaveId { get; internal set; }

    private static CraftingSystem _craftingSystem;
}