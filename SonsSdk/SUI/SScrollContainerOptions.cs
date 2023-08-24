using MonoMod.Utils;
using SonsSdk;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SUI;

public class SScrollContainerOptions : SContainerOptions
{
    public SContainerOptions ContainerObject;

    public SScrollContainerOptions(GameObject root) : base(root)
    {
        ContainerObject = new SContainerOptions(root.transform.Find("Viewport/Content").gameObject);
        Object.DestroyImmediate(root.GetComponent<Image>());
    }
    
    public SScrollContainerOptions ContainerPadding(int padding)
    {
        ContainerObject.Padding(padding);
        return this;
    }
    
    public SScrollContainerOptions ContainerPadding(int left, int right, int top, int bottom)
    {
        ContainerObject.Padding(left, top, right, bottom);
        return this;
    }

    public override SScrollContainerOptions Add(SUiElement element)
    {
        element.SetParent(ContainerObject);
        
        // add ids from child
        ChildIds.AddRange(element.ChildIds);
        if (!string.IsNullOrEmpty(element._id))
            ChildIds[element._id] = element;
        
        // add classes from child
        foreach (var (cls, elements) in element.ChildClasses)
        {
            if (!ChildClasses.ContainsKey(cls))
                ChildClasses[cls] = new List<SUiElement>();
            ChildClasses[cls].AddRange(elements);
        }
        
        foreach (var cls in element._classes)
        {
            if (!ChildClasses.ContainsKey(cls))
                ChildClasses[cls] = new List<SUiElement>();
            ChildClasses[cls].Add(element);
        }

        return this;
    }
}