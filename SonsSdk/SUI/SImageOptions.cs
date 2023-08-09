using UnityEngine;
using UnityEngine.UI;

namespace SUI;

public class SImageOptions : SUiElement<SImageOptions, Texture>
{
    public RawImage ImageObject;

    public SImageOptions(GameObject root) : base(root)
    {
        ImageObject = GetOrAdd<RawImage>();
    }

    public SImageOptions Texture(Texture texture)
    {
        ImageObject.texture = texture;
        return this;
    }

    protected override void OnObservaleChanged(Texture value)
    {
        if (ImageObject.texture == value)
            return;
        
        ImageObject.texture = value;
    }
}