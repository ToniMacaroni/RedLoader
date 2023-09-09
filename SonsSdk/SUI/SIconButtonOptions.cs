using RedLoader;
using UnityEngine;
using UnityEngine.UI;

namespace SUI;

public class SIconButtonOptions : SBgButtonOptions
{
    public Image IconObject;
    
    public SIconButtonOptions(GameObject root) : base(root)
    {
        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(root.transform, false);
        IconObject = iconGo.AddComponent<Image>();
        IconObject.raycastTarget = false;
        IconObject.preserveAspect = true;
        IconObject.rectTransform.anchorMin = new Vector2(0f, 0f);
        IconObject.rectTransform.anchorMax = new Vector2(1f, 1f);
        IconObject.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        IconObject.rectTransform.sizeDelta = new Vector2(0, 0);

        IconObject.type = Image.Type.Simple;
    }
    
    public SIconButtonOptions Icon(Sprite sprite)
    {
        ImageObject.sprite = sprite;
        return this;
    }

    public SIconButtonOptions NoText()
    {
        TextObject.gameObject.SetActive(false);
        return this;
    }
}