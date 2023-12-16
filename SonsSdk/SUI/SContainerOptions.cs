using UnityEngine;
using UnityEngine.UI;

namespace SUI;

public class SContainerOptions : SUiElement<SContainerOptions>
{
    private bool _observableTogglesGameObject = true;

    public SContainerOptions(GameObject root) : base(root)
    { }
    
    protected override void VisibilityObservalbleChanged(bool value)
    {
        if(InvertVisibility)
            value = !value;
        
        if (_observableTogglesGameObject)
        {
            if(value == Root.activeSelf)
                return;
            Root.SetActive(value);
            return;
        }
        
        var canvasGroup = Root.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            return;
        
        canvasGroup.alpha = value ? 1 : 0;
        canvasGroup.interactable = value;
    }
    
    /// <summary>
    /// Binds the visibility of the container to an observable boolean value.
    /// </summary>
    /// <param name="observable">The observable boolean value to bind to.</param>
    /// <param name="toggleGameObject">Whether to toggle the GameObject's active state based on the observable value or the canvasgroup alpha.</param>
    public SContainerOptions BindVisibility(Observable<bool> observable, bool toggleGameObject)
    {
        UnbindVisibility();

        InvertVisibility = false;
        VisibilityObservable = observable;
        _observableTogglesGameObject = toggleGameObject;
        observable.OnValueChanged += VisibilityObservalbleChanged;
        VisibilityObservalbleChanged(observable.Value);
        
        return this;
    }
    
    /// <summary>
    /// Binds the visibility of the container to an observable boolean value.
    /// </summary>
    /// <param name="observable">The observable boolean value to bind to.</param>
    /// <param name="toggleGameObject">Whether to toggle the GameObject's active state based on the observable value or the canvasgroup alpha.</param>
    public SContainerOptions BindVisibilityInverted(Observable<bool> observable, bool toggleGameObject)
    {
        UnbindVisibility();
        
        InvertVisibility = true;
        VisibilityObservable = observable;
        _observableTogglesGameObject = toggleGameObject;
        observable.OnValueChanged += VisibilityObservalbleChanged;
        VisibilityObservalbleChanged(observable.Value);
        
        return this;
    }

    /// <summary>
    /// Configures the container's layout as horizontal with optional spacing and layout mode.
    /// </summary>
    /// <param name="spacing">Optional. The amount of spacing between elements.</param>
    /// <param name="mode">Optional. The layout mode to apply (e.g., flexible, fixed, etc.).</param>
    public SContainerOptions Horizontal(float spacing = 0, string mode = null)
    {
        var layout = Root.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        if (!string.IsNullOrEmpty(mode))
            LayoutMode(mode);
        return this;
    }

    /// <summary>
    /// Configures the container's layout as vertical with optional spacing and layout mode.
    /// </summary>
    /// <param name="spacing">Optional. The amount of spacing between elements.</param>
    /// <param name="mode">Optional. The layout mode to apply (e.g., flexible, fixed, etc.).</param>
    public SContainerOptions Vertical(float spacing = 0, string mode = null)
    {
        var layout = Root.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        if (!string.IsNullOrEmpty(mode))
            LayoutMode(mode);
        return this;
    }
    
    /// <summary>
    /// Sets the alignment of child elements within the container's horizontal or vertical layout.
    /// </summary>
    /// <param name="alignment">The alignment for child elements.</param>
    public SContainerOptions LayoutChildAlignment(TextAnchor alignment)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.childAlignment = alignment;
        return this;
    }
    
    /// <summary>
    /// Configures whether child elements in the container's horizontal or vertical layout should control their width and/or height.
    /// </summary>
    /// <param name="width">Optional. Set to true to enable child width control, false to disable. Set to null to keep the current setting.</param>
    /// <param name="height">Optional. Set to true to enable child height control, false to disable. Set to null to keep the current setting.</param>
    public SContainerOptions ChildControl(bool? width = null, bool? height = null)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.childControlWidth = width ?? layout.childControlWidth;
        layout.childControlHeight = height ?? layout.childControlHeight;
        return this;
    }
    
    /// <summary>
    /// Configures whether child elements in the container's horizontal or vertical layout should expand to fill available space.
    /// </summary>
    /// <param name="width">Optional. Set to true to enable child width expansion, false to disable. Set to null to keep the current setting.</param>
    /// <param name="height">Optional. Set to true to enable child height expansion, false to disable. Set to null to keep the current setting.</param>
    public SContainerOptions ChildExpand(bool? width = null, bool? height = null)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.childForceExpandWidth = width ?? layout.childForceExpandWidth;
        layout.childForceExpandHeight = height ?? layout.childForceExpandHeight;
        return this;
    }
    
    /// <summary>
    /// Configures whether child elements in the container's horizontal or vertical layout should use child scale settings.
    /// </summary>
    /// <param name="width">Optional. Set to true to enable child width scaling, false to disable. Set to null to keep the current setting.</param>
    /// <param name="height">Optional. Set to true to enable child height scaling, false to disable. Set to null to keep the current setting.</param>
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
    
    /// <summary>
    /// Sets the spacing between elements in the container's horizontal or vertical layout.
    /// </summary>
    /// <param name="spacing">The amount of spacing between elements.</param>
    public virtual SContainerOptions Spacing(float spacing)
    {
        var layout = Root.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (!layout)
            return this;
        
        layout.spacing = spacing;
        return this;
    }
    
    /// <summary>
    /// Sets the spacing between rows and columns in the container's grid layout.
    /// </summary>
    /// <param name="spacingRow">The amount of spacing between rows.</param>
    /// <param name="spacingCol">The amount of spacing between columns.</param>
    public virtual SContainerOptions Spacing(float spacingRow, float spacingCol)
    {
        var layout = Root.GetComponent<GridLayoutGroup>();
        if (!layout)
            return this;
        
        layout.spacing = new Vector2(spacingRow, spacingCol);
        return this;
    }

    /// <summary>
    /// Sets equal padding for all sides of the container's layout.
    /// </summary>
    /// <param name="padding">The amount of padding to apply on all sides.</param>
    public SContainerOptions Padding(float padding)
    {
        var layout = Root.GetComponent<LayoutGroup>();
        if (!layout)
            return this;
        
        layout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        return this;
    }
    
    /// <summary>
    /// Sets padding for the container's layout on all sides individually.
    /// </summary>
    /// <param name="left">The amount of padding for the left side.</param>
    /// <param name="right">The amount of padding for the right side.</param>
    /// <param name="top">The amount of padding for the top side.</param>
    /// <param name="bottom">The amount of padding for the bottom side.</param>
    public SContainerOptions Padding(float left, float right, float top, float bottom)
    {
        var layout = Root.GetComponent<LayoutGroup>();
        if (!layout)
            return this;
        
        layout.padding = new RectOffset((int)left, (int)right, (int)top, (int)bottom);
        return this;
    }
    
    /// <summary>
    /// Sets horizontal padding for the container's layout.
    /// </summary>
    /// <param name="padding">The amount of horizontal padding to apply.</param>
    public SContainerOptions PaddingHorizontal(float padding)
    {
        var layout = Root.GetComponent<LayoutGroup>();
        if (!layout)
            return this;
        
        layout.padding = new RectOffset((int)padding, (int)padding, layout.padding.top, layout.padding.bottom);
        return this;
    }
    
    /// <summary>
    /// Sets vertical padding for the container's layout.
    /// </summary>
    /// <param name="padding">The amount of vertical padding to apply.</param>
    public SContainerOptions PaddingVertical(float padding)
    {
        var layout = Root.GetComponent<LayoutGroup>();
        if (!layout)
            return this;
        
        layout.padding = new RectOffset(layout.padding.left, layout.padding.right, (int)padding, (int)padding);
        return this;
    }

    /// <summary>
    /// Creates a grid layout with the given constraint count and spacing.
    /// </summary>
    /// <param name="constraintCount">Number of the fixed rows or columns</param>
    /// <param name="spacing"></param>
    public SContainerOptions Grid(int constraintCount, float spacing = 0)
    {
        var layout = Root.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = constraintCount;
        layout.spacing = new Vector2(spacing, spacing);
        return this;
    }
    
    /// <summary>
    /// Sets the cell size of the GridLayoutGroup attached to the root object.
    /// </summary>
    /// <param name="width">Width of the cell.</param>
    /// <param name="height">Height of the cell.</param>
    public SContainerOptions CellSize(float width, float height)
    {
        var layout = Root.GetComponent<GridLayoutGroup>();
        if (!layout)
            return this;
        
        layout.cellSize = new Vector2(width, height);
        return this;
    }

    /// <summary>
    /// Configures automatic sizing for the container using ContentSizeFitter.
    /// </summary>
    /// <param name="horizontal">The horizontal fitting mode for content.</param>
    /// <param name="vertical">The vertical fitting mode for content.</param>
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
    
    public SContainerOptions CanvasGroup(float alpha = 1, bool interactable = true)
    {
        var group = GetOrAdd<CanvasGroup>();
        group.interactable = interactable;
        group.alpha = alpha;
        return this;
    }
    
    public SContainerOptions Opacity(float alpha)
    {
        var group = GetOrAdd<CanvasGroup>();
        group.alpha = alpha;
        return this;
    }

    /// <summary>
    /// Sets the background appearance of the container using a solid color and an optional background sprite.
    /// </summary>
    /// <param name="color">The desired background color.</param>
    /// <param name="clean">Flag to determine if the background sprite should be removed (optional).</param>
    public SContainerOptions Background(Color color, EBackground type = EBackground.Sons, Image.Type? spriteType = null)
    {
        var image = GetOrAdd<Image>();
        
        image.sprite = SUI.GetBackgroundSprite(type);
        if(spriteType.HasValue)
            image.type = spriteType.Value;
        else
            image.type = type is EBackground.None or EBackground.Sons ? Image.Type.Simple : Image.Type.Sliced;

        image.color = color;
        return this;
    }
    
    public SContainerOptions Background(SUI.BackgroundDefinition backgroundDefinition)
    {
        var image = GetOrAdd<Image>();
        
        backgroundDefinition.ApplyTo(image);
        
        return this;
    }
    
    public SContainerOptions Material(Material material)
    {
        if (!Root.TryGetComponent(out Image image))
            return this;
        
        image.material = material;
        
        return this;
    }
    
    public SContainerOptions Background(string color, bool clean = false)
    {
        var image = GetOrAdd<Image>();

        image.color = SUI.ColorFromString(color);
        
        if(clean)
            image.sprite = null;
        
        return this;
    }

    public SContainerOptions Background(bool show)
    {
        var bg = Root.GetComponent<Image>();
        if (bg)
            bg.enabled = show;
        
        return this;
    }

    /// <summary>
    /// Sets the background appearance of the container using a solid color and an optional background sprite.
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="color">The desired background color.</param>
    /// <param name="type"></param>
    public SContainerOptions Background(Sprite sprite, Color? color, Image.Type type = Image.Type.Simple)
    {
        var image = GetOrAdd<Image>();

        image.sprite = sprite;
        image.type = type;

        if(color.HasValue)
            image.color = color.Value;
        return this;
    }

    /// <summary>
    /// Gets or adds a canvas and overrides the sorting order of the parent canvas.
    /// </summary>
    /// <param name="sortingOrder"></param>
    /// <returns></returns>
    public SContainerOptions OverrideSorting(int sortingOrder)
    {
        var canvas = GetOrAdd<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        GetOrAdd<GraphicRaycaster>();
        return this;
    }

    public static SContainerOptions operator -(SContainerOptions container, SUiElement element)
    {
        return container.Add(element);
    }
    
    public SContainerOptions this[SUiElement element]
    {
        get => Add(element);
    }
}