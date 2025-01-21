using SonsSdk.AssetImporting.GLTF;
using UnityGLTF;

namespace SonsSdk.AssetImporting;

public class AssetImportingInitializer
{
    public static void Init()
    {
        // register custom gltf plugins
        GLTFSettings.RegisterPlugin<IngredientImport>();
    }
}