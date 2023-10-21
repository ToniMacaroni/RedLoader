using Construction;

namespace SonsSdk;

public static class GameState
{
    public static ConstructionManager ConstructionManager => RepositioningUtils.Manager;
    
    public static uint LastLoadedSaveId { get; internal set; }
}