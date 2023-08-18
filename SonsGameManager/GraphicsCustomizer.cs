using AdvancedTerrainGrass;
using SFLoader;

namespace SonsGameManager;

public class GraphicsCustomizer
{
    public static GraphicsCustomizer Instance { get; private set; }
    
    public ConfigCategory Category { get; private set; }
    
    public ConfigEntry<float> GrassDensity { get; private set; }
    public ConfigEntry<float> GrassDistance { get; private set; }

    private GraphicsCustomizer()
    {
        Category = ConfigSystem.CreateCategory("graphics", "Graphics");
        Category.SetFilePath(ConfigSystem.GetFilePath("_SFLoader_Gfx.cfg"));
        
        GrassDensity = Category.CreateEntry(
            "grass_density", 
            0.5f, 
            "Grass Density", 
            "The density of the grass.");
        
        GrassDistance = Category.CreateEntry(
            "grass_distance",
            120f,
            "Grass Distance",
            "The distance of the grass.");
    }
    
    public static void Load()
    {
        Instance = new GraphicsCustomizer();
    }

    public static void Apply()
    {
        if(Instance == null)
            return;

        if (GrassManager._instance)
        {
            SetGrassSettings(Instance.GrassDensity.Value, Instance.GrassDistance.Value);
        }
    }

    public static void SetGrassSettings(float density, float distance)
    {
        if(!GrassManager._instance)
            return;
        var inst = GrassManager._instance;
        
        GrassManager._instance.RefreshGrassRenderingSettings(density,
            distance,
            inst.FadeLength,
            inst.CacheDistance,
            inst.SmallDetailFadeStart,
            inst.SmallDetailFadeLength,
            inst.DetailFadeStart,
            inst.DetailFadeLength,
            inst.ShadowStart,
            inst.ShadowFadeLength,
            inst.ShadowStartFoliage,
            inst.ShadowFadeLengthFoliage,
            false);
    }
}