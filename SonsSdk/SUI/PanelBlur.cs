using RedLoader;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace SUI;

public static class PanelBlur
{
    private static Dictionary<Color, Material> _blurMaterials = new();
    internal static UIBlurPass CurrentBlurPass;

    public static Material GetForColor(Color color)
    {
        if (_blurMaterials.TryGetValue(color, out var blurMat))
            return blurMat;

        var newMat = new Material(Shader.Find("Sons/UI/SampleBlurHLSL"));
        newMat.color = color;
        _blurMaterials[color] = newMat;
        return newMat;
    }

    public static Material GetForShade(float shade)
    {
        return GetForColor(new(shade, shade, shade));
    }

    internal static void SetRadius(float radius)
    {
        if (CurrentBlurPass == null)
            return;

        CurrentBlurPass.radius = radius;
    }
    
    internal static void SetupBlur()
    {
        if (!LocalPlayer.Inventory)
            return;

        var invBlur = LocalPlayer.Inventory.CameraController.Camera.transform.Find("SlightBlurPass");
        
        if(!invBlur)
        {
            RLog.Error("Failed to find SlightBlurPass!");
            return;
        }

        var go = UnityEngine.Object.Instantiate(invBlur.gameObject, LocalPlayer.MainCamTr, false);

        try
        {
            var volume = go.GetComponent<CustomPassVolume>();
            CurrentBlurPass = volume.customPasses._items[0].Cast<UIBlurPass>();
            CurrentBlurPass.radius = 25;
        }
        catch (Exception e)
        {
            RLog.Error($"Failed to initialize UIBlurPass! {e}");
        }
    }
}