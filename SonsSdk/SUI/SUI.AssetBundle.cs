using System.Diagnostics;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using MonoMod.Utils;
using RedLoader;
using RedLoader.Utils;
using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Color = System.Drawing.Color;
using Debug = System.Diagnostics.Debug;
using Debugger = Il2CppSystem.Diagnostics.Debugger;
using Object = UnityEngine.Object;

namespace SUI;

public partial class SUI
{
    private static AssetBundle _assetBundle;

    private static GameObject _colorWheelPrefab;

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

        // ClassInjector.RegisterTypeInIl2Cpp<ColorPicker>(new()
        // {
        //     Interfaces = new[]
        //     {
        //         typeof(IPointerDownHandler),
        //         typeof(IDragHandler),
        //         typeof(IPointerUpHandler),
        //         typeof(IPointerClickHandler)
        //     }
        // });
        
        ClassInjector.RegisterTypeInIl2Cpp<ColorPicker>();

        RLog.DebugBig("Loading bundle content");
        foreach (var asset in AssetBundle.LoadAllAssets())
        {
            if (asset.name is "atlas")
            {
                TMP_Settings.defaultSpriteAsset = new TMP_SpriteAsset(asset.Pointer);
                continue;
            }

            if (asset.name is "forkawesome")
            {
                if (TMP_Settings.fallbackFontAssets == null)
                    TMP_Settings.fallbackFontAssets = new();
                TMP_Settings.fallbackFontAssets.Add(new TMP_FontAsset(asset.Pointer));
                RLog.Msg(Color.Orange, $"Loaded additional fallback font: {asset.name}");
                continue;
            }

            if (asset.name is "ColorPicker")
            {
                _colorWheelPrefab = new GameObject(asset.Pointer);
                continue;
            }

            RLog.Debug($"Adding sprite: {asset.name}");
            GameResources.Sprites[asset.name] = new Sprite(asset.Pointer);
        }

        RLog.DebugBig("Loaded bundle content");
    }
}