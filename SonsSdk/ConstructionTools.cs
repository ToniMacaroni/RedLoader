using Construction;
using Sons.Crafting.Structures;

namespace SonsSdk;

public class ConstructionTools
{
    public static StructureRecipe GetRecipe(int id)
    {
        if (!ScrewStructureManager._instance)
        {
            return null;
        }
        
        return ScrewStructureManager._instance._database._recipes._items.FirstOrDefault(x => x._id == id);
    }
    
    public static StructureRecipe GetRecipe(string name)
    {
        if (!ScrewStructureManager._instance)
        {
            return null;
        }
        
        return ScrewStructureManager._instance._database._recipes._items.FirstOrDefault(x => x.Name == name);
    }

    public static ElementProfile GetProfile(int id)
    {
        if (!ElementProfileDatabase._instance)
        {
            return null;
        }
        
        return ElementProfileDatabase._instance._profiles._items.FirstOrDefault(x => x._id == id);
    }
}