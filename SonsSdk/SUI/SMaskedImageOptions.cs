using SonsSdk;
using UnityEngine;
using UnityEngine.UI;

namespace SUI;

public class SMaskedImageOptions : SUiElement<SMaskedImageOptions>
{
    public RawImage ImageObject;
    
    public SMaskedImageOptions(GameObject root) : base(root)
    {
        ImageObject = root.FindGet<RawImage>("RawImage");
    }
    
    public SMaskedImageOptions Texture(Texture2D texture)
    {
        ImageObject.texture = texture;
        return this;
    }
}