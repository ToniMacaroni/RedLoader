# Asset Bundles
Asset bundles and Addressables allow you to load your own assets into the game.  
For more info see [here](https://docs.unity3d.com/Manual/AssetBundlesIntro.html).  

Redloader can load and map asset bundles automatically. For it to work do the following:
1) Put your asset bundle into the `Mods/<ModName>/` folder
2) Add a static class like the following to your mod:
```csharp
[AssetBundle("Bundle")]
public static class AssetBundleTest
{
    [AssetReference("Asset")]
    public static GameObject Asset { get; set; }
}
```

The `[AssetBundle]` attribute is the name of your asset bundle. In this example the bundle is at this location `Mods/<ModName>/Bundle`.
The `[AssetReference]` attribute is the name of the asset in the bundle. Make sure the attribute is on a **property** that has a **getter and a setter**.

# Addressables
Addressables also allow you to load your own assets into the game. See [here](https://docs.unity3d.com/Packages/com.unity.addressables@1.20/manual/RuntimeAddressables.html).
To load in your addressables put the catalog and the bundle into the `Mods/<ModName>/` folder.  
RedLoader will automatically register the catalog. After that you can load your asset like this:
```csharp
AssetLoaders.LoadAsset<Sprite>("MyAsset");
AssetLoaders.LoadPrefab("MyAsset"); // same as AssetLoaders.LoadAsset<GameObject>("");
AssetLoaders.InstantiatePrefab("MyAsset"); // same as Object.Instantiate(AssetLoaders.LoadAsset<GameObject>(""));
```

**Important:** Make sure to use a relative path for the bundle load path in the  catalog.
This will load the bundle relative to the Sons of the Forest executable. So the path should be something like `Mods/<ModName>/`.

*More info on addressables will follow soon*