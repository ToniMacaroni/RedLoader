using UnityEngine;
using UnityEngine.UI;

namespace SUI;

public class SSpriteOptions : SUiElement<SSpriteOptions, Sprite>
{
    public Image ImageObject;

    public SSpriteOptions(GameObject root) : base(root)
    {
        ImageObject = GetOrAdd<Image>();
    }

    public SSpriteOptions Sprite(Sprite sprite)
    {
        ImageObject.sprite = sprite;
        return this;
    }
    
    public SSpriteOptions Color(Color color)
    {
        ImageObject.color = color;
        return this;
    }

    protected override void OnObservaleChanged(Sprite value)
    {
        if (ImageObject.sprite == value)
            return;
        
        ImageObject.sprite = value;
    }
}