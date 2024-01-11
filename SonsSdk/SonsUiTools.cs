using Sons.Gui;
using Sons.Gui.Input;
using SUI;
using TMPro;
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
        Register(new()
        {
            Id = id,
            Container = container,
            IdHash = UiElement.GetHash(id),
            TransformType = transformType,
            MaxPooled = maxPooled
        });
    }
    
    /// <summary>
    /// Registers a custom 2d ui to the sons link ui manager. Should be called in the OnSdkInitialized callback.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="id"></param>
    public static void RegisterCustom2DUi(SContainerOptions container,
        string id,
        UiElement.UiElementTransformType transformType = UiElement.UiElementTransformType.Auto)
    {
        Register(new()
        {
            Id = id,
            Container = container,
            IdHash = UiElement.GetHash(id),
            TransformType = transformType,
            MaxPooled = 1,
            Is2d = true
        });
    }

    internal static void Register(CustomUiElement element)
    {
        if(CustomLinkElements.Any(x => x.Id == element.Id))
        {
            return;
        }

        element.Container.Root.DontDestroyOnLoad();
        element.Container.Active(false);

        CustomLinkElements.Add(element);
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
        var prefab3d = UiManager._instance.FindElement("screen.use", null);
        var prefab2d = UiManager._instance.FindElement("construction.Place", null);
        
        foreach (var element in CustomLinkElements)
        {
            var uiElement = new UiElement(element.Is2d ? prefab2d : prefab3d);
            uiElement._id = element.Id;
            uiElement._idHash = element.IdHash;
            uiElement._transformType = element.TransformType;
            uiElement._maxPooled = element.MaxPooled;
            uiElement._target = Object.Instantiate(element.Container.Root, element.Is2d 
                ? prefab2d._target.transform.parent 
                : prefab3d._target.transform.parent, false);
            uiElement._rootParent = element.Is2d ? prefab2d._rootParent : prefab3d._rootParent;
            if(element.Is2d)
                uiElement._tempParent = prefab2d._tempParent;
            
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
        public bool Is2d;
    }
}