using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace SUI;

public class SLabelDividerOptions : SUiElement<SLabelDividerOptions>
{
    public SLabelDividerOptions(GameObject root) : base(root)
    {
        TextObject = root.FindGet<TextMeshProUGUI>("ScreenLabel");
        TextObject.gameObject.Destroy<LocalizeStringEvent>();
    }
}