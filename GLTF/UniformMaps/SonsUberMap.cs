using UnityEngine;

namespace UnityGLTF;

public class SonsUberMap : MetalRoughMap
{
    public SonsUberMap(int maxLod) : base("Sons/Uber", maxLod)
    { }

    public SonsUberMap(string shader, int maxLod) : base(shader, maxLod)
    { }

    public override Texture BaseColorTexture
    {
        get => _material.GetTexture("_BaseColorMap");
        set => _material.SetTexture("_BaseColorMap", value);
    }

    public override Texture NormalTexture
    {
        get => _material.GetTexture("_NormalMap");
        set => _material.SetTexture("_NormalMap", value);
    }
    
    // TODO: Check why the normal map isn't working correctly
    // public override Texture NormalTexture
    // {
    //     get => null;
    //     set {}
    // }
    
    public override double NormalTexScale
    {
        get => _material.GetFloat("_NormalScale");
        set => _material.SetFloat("_NormalScale", (float)value);
    }

    public override Texture OcclusionTexture
    {
        get => null;
        set {}
    }

    public override double OcclusionTexStrength
    {
        get => 1;
        set {}
    }

    public Texture MaskMap
    {
        get => _material.GetTexture("_MaskMap");
        set => _material.SetTexture("_MaskMap", value);
    }
}