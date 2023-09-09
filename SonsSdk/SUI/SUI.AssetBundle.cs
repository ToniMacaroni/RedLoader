using System.Reflection;
using Il2CppInterop.Runtime;
using MonoMod.Utils;
using RedLoader;
using RedLoader.Utils;
using SonsSdk;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SUI;

public partial class SUI
{
    private static AssetBundle _assetBundle;

    public static AssetBundle AssetBundle
    {
        get
        {
            if (!_assetBundle)
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SonsSdk.Resources.bundle");
                var bytes = new byte[stream!.Length];
                _ = stream.Read(bytes, 0, bytes.Length);
                _assetBundle = AssetBundle.LoadFromMemory(bytes);
            }

            return _assetBundle;
        }
    }

    private static void InitBundleContent()
    {
        TMP_SpriteAsset atlas = null;
        
        RLog.DebugBig("Loading bundle content");
        foreach (var asset in AssetBundle.LoadAllAssets())
        {
            if (asset.name is "atlas")
            {
                atlas = new TMP_SpriteAsset(asset.Pointer);
                continue;
            }
            
            RLog.Debug($"Adding sprite: {asset.name}");
            _sprites[asset.name] = new Sprite(asset.Pointer);
        }
        
        RLog.Debug($"Original default sprite asset: {TMP_Settings.defaultSpriteAsset?.name}");
        TMP_Settings.defaultSpriteAsset = atlas;
        RLog.DebugBig("Loaded bundle content");
    }
}