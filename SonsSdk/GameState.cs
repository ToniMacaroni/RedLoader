using Construction;
using Sons.Crafting;
using TheForest;
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
    
    /// <summary>
    /// Returns true if the player is in the game world.
    /// </summary>
    public static bool IsInGame => LocalPlayer._instance;
    
    public static bool IsInConsole => DebugConsole.Instance?._showConsole ?? false;
    
    /// <summary>
    /// returns true if the player is not in the crafting book, console etc.
    /// Useful for example when a hotkey shouldn't be accidentally triggered in the console.
    /// </summary>
    public static bool IsPlayerControllable => !IsInConsole && LocalPlayer.IsInWorld;

    private static CraftingSystem _craftingSystem;
}