using System.Reflection;
using MonoMod.Utils;
using RedLoader.Utils;
using UnityEngine;

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
        foreach (var asset in AssetBundle.LoadAllAssets())
        {
            _sprites.Add(asset.name, new Sprite(asset.Pointer));
        }
    }
}