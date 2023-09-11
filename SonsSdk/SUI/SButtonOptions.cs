using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SUI;

public class SButtonOptions : SUiElement<SButtonOptions>
{
    public Button ButtonObject;

    public SButtonOptions(GameObject root) : base(root)
    {
        ButtonObject = root.GetComponent<Button>();
        TextObject = root.FindGet<TextMeshProUGUI>("ContentPanel/TextBase");

        RectTransform.offsetMin = RectTransform.offsetMax = new Vector2(0, 0);
        RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0, 0);
        root.SetActive(true);
    }

    public SButtonOptions Notify(Action action)
    {
        ButtonObject.onClick.AddListener(action);
        return this;
    }
    
    public SButtonOptions Notify(Action<SButtonOptions> action)
    {
        void OnClickImpl()
        {
            action(this);
        }
        
        ButtonObject.onClick.AddListener((UnityAction)OnClickImpl);
        return this;
    }
}