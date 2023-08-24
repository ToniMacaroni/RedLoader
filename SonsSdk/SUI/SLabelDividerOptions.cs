using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SUI;

public class SLabelDividerOptions : SUiElement<SLabelDividerOptions>
{
    public RawImage ImageObject;
    
    public SLabelDividerOptions(GameObject root) : base(root)
    {
        TextObject = root.FindGet<TextMeshProUGUI>("ScreenLabel");
        ImageObject = root.FindGet<RawImage>("DividerPanel/Divider");
        root.SetActive(true);
    }

    public SLabelDividerOptions Texture(Texture2D tex)
    {
        ImageObject.texture = tex;
        return this;
    }

    public SLabelDividerOptions Spacing(float spacing)
    {
        Root.GetComponent<HorizontalLayoutGroup>().spacing = spacing;
        return this;
    }
}