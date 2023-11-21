using Sons.Gui;
using Sons.Gui.Input;
using SUI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SonsSdk;

public class SonsUiTools
{
    internal static List<CustomUiElement> CustomLinkElements = new();

    /// <summary>
    /// Registers a custom ui to the sons link ui manager. Should be called in the OnSdkInitialized callback.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="id"></param>
    public static void RegisterCustomLinkUi(SContainerOptions container,
        string id,
        UiElement.UiElementTransformType transformType = UiElement.UiElementTransformType.Auto,
        int maxPooled = 1)
    {
        if(CustomLinkElements.Any(x => x.Id == id))
        {
            return;
        }

        container.Root.DontDestroyOnLoad();
        container.Active(false);
        
        var elem = new CustomUiElement()
        {
            Id = id,
            Container = container,
            IdHash = UiElement.GetHash(id),
            TransformType = transformType,
            MaxPooled = maxPooled
        };
        
        CustomLinkElements.Add(elem);
    }

    public static void AddLinkUi(GameObject gameObject, string id, float distance = 15f, bool refresh = false)
    {
        if(refresh)
            gameObject.SetActive(false);

        var linkUi = gameObject.AddComponent<LinkUiElement>();
        linkUi._maxDistance = distance;
        linkUi.SetId(id, refresh);
        
        if(refresh)
            gameObject.SetActive(true);
    }

    public static Transform CreateLinkUi(Transform parent, string id, float distance = 15f, bool refresh = false)
    {
        var go = new GameObject("InteractionUi");
        go.transform.SetParent(parent, false);
        AddLinkUi(go, id, distance, refresh);
        return go.transform;
    }

    internal static void Init()
    {
        SdkEvents.OnGameActivated.Subscribe(BeforeLoadSave);
    }
    
    internal static void BeforeLoadSave()
    {
        var prefab = UiManager._instance.FindElement("screen.use", null);
        var parent = prefab._target.transform.parent;
        
        foreach (var element in CustomLinkElements)
        {
            var uiElement = new UiElement(prefab);
            uiElement._id = element.Id;
            uiElement._idHash = element.IdHash;
            uiElement._transformType = element.TransformType;
            uiElement._maxPooled = element.MaxPooled;
            uiElement._target = Object.Instantiate(element.Container.Root, parent);
            uiElement._rootParent = prefab._rootParent;
            
            UiManager._instance._elements.Add(uiElement);
        }
    }
    
    public class CustomUiElement
    {
        public string Id;
        public SContainerOptions Container;
        public int IdHash;
        public UiElement.UiElementTransformType TransformType;
        public int MaxPooled;
    }
}