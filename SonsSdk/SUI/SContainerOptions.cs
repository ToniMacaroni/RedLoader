using System;
using UnityEngine;
using UnityEngine.UI;

namespace SUI;

public class SContainerOptions : SUiElement<SContainerOptions>
{
    public SContainerOptions(GameObject root) : base(root)
    { }

    public SContainerOptions Horizontal(float spacing = 0)
    {
        var layout = Root.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        return this;
    }

    public SContainerOptions Vertical(float spacing = 0)
    {
        var layout = Root.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        return this;
    }
    
    public SContainerOptions LayoutChildAlignment(TextAnchor alignment)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.childAlignment = alignment;
        return this;
    }
    
    public SContainerOptions ChildControl(bool? width = null, bool? height = null)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.childControlWidth = width ?? layout.childControlWidth;
        layout.childControlHeight = height ?? layout.childControlHeight;
        return this;
    }
    
    public SContainerOptions ChildExpand(bool? width = null, bool? height = null)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.childForceExpandWidth = width ?? layout.childForceExpandWidth;
        layout.childForceExpandHeight = height ?? layout.childForceExpandHeight;
        return this;
    }
    
    public SContainerOptions LayoutUseChildScale(bool? width = null, bool? height = null)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.childScaleWidth = width ?? layout.childScaleWidth;
        layout.childScaleHeight = height ?? layout.childScaleHeight;
        return this;
    }
    
    /// <summary>
    /// E = Expand, C = Control.
    /// First letter is width, second is height
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public SContainerOptions LayoutMode(string mode = "ee")
    {
        mode = mode.ToLower();
        
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;

        layout.childControlWidth = mode[0] switch
        {
            'e' or 'c' => true,
            _ => false
        };
        
        layout.childControlHeight = mode[1] switch
        {
            'e' or 'c' => true,
            _ => false
        };
        
        layout.childForceExpandWidth = mode[0] switch
        {
            'e' => true,
            _ => false
        };
        
        layout.childForceExpandHeight = mode[1] switch
        {
            'e' => true,
            _ => false
        };
        
        return this;
    }
    
    public SContainerOptions Spacing(float spacing)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.spacing = spacing;
        return this;
    }
    
    public SContainerOptions Padding(float padding)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        return this;
    }
    
    public SContainerOptions Padding(float left, float right, float top, float bottom)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.padding = new RectOffset((int)left, (int)right, (int)top, (int)bottom);
        return this;
    }
    
    public SContainerOptions PaddingHorizontal(float padding)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.padding = new RectOffset((int)padding, (int)padding, layout.padding.top, layout.padding.bottom);
        return this;
    }
    
    public SContainerOptions PaddingVertical(float padding)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.padding = new RectOffset(layout.padding.left, layout.padding.right, (int)padding, (int)padding);
        return this;
    }

    public SContainerOptions Grid(int columns, float spacing = 0)
    {
        var layout = Root.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = columns;
        layout.spacing = new Vector2(spacing, spacing);
        return this;
    }

    public SContainerOptions AutoSize(ContentSizeFitter.FitMode horizontal, ContentSizeFitter.FitMode vertical)
    {
        var layout = GetOrAdd<ContentSizeFitter>();
        layout.horizontalFit = horizontal;
        layout.verticalFit = vertical;
        return this;
    }

    /// <summary>
    /// M = MinSize, P = PreferredSize.
    /// First letter is width, second is height
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public SContainerOptions AutoSize(string mode = "XX")
    {
        mode = mode.ToLower();
        
        if(mode.Length != 2)
            throw new Exception("Mode must be 2 characters long");
        
        var layout = GetOrAdd<ContentSizeFitter>();
        layout.horizontalFit = mode[0] switch
        {
            'm' => ContentSizeFitter.FitMode.MinSize,
            'p' => ContentSizeFitter.FitMode.PreferredSize,
            _ => ContentSizeFitter.FitMode.Unconstrained
        };
        
        layout.verticalFit = mode[1] switch
        {
            'm' => ContentSizeFitter.FitMode.MinSize,
            'p' => ContentSizeFitter.FitMode.PreferredSize,
            _ => ContentSizeFitter.FitMode.Unconstrained
        };
        
        return this;
    }

    public SContainerOptions Background(Color color, bool clean = false)
    {
        var image = GetOrAdd<Image>();
        
        if(!clean)
            image.sprite = SUI.GetBackgroundSprite();
        
        image.color = color;
        return this;
    }

    public SContainerOptions Add(SUiElement element)
    {
        element.SetParent(this);
        return this;
    }

    public static SContainerOptions operator -(SContainerOptions container, SUiElement element)
    {
        return container.Add(element);
    }
    
    public static SContainerOptions operator >(SContainerOptions container, SUiElement element)
    {
        
        return container;
    }
    
    public static SContainerOptions operator <(SContainerOptions container, SUiElement element)
    {
        return container.Add(element);
    }
}