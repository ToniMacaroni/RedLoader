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

        // Recipes in the list can be null.
        foreach (var recipe in ScrewStructureManager._instance._database._recipes)
        {
            if(!recipe)
                continue;

            if (recipe._id == id)
                return recipe;
        }

        return null;
    }
    
    public static StructureRecipe GetRecipe(string name)
    {
        if (!ScrewStructureManager._instance)
        {
            return null;
        }
        
        // Recipes in the list can be null.
        foreach (var recipe in ScrewStructureManager._instance._database._recipes)
        {
            if(!recipe)
                continue;

            if (recipe.Name == name)
                return recipe;
        }

        return null;
    }
    
    public static ElementProfile GetProfile(int id)
    {
        if (!ElementProfileDatabase._instance)
        {
            return null;
        }
        
        // Profiles in the list can be null.
        foreach (var profile in ElementProfileDatabase._instance._profiles)
        {
            if(!profile)
                continue;

            if (profile._id == id)
                return profile;
        }

        return null;
    }
}