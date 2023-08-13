using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SUI;

public class SUiElement
{
    public RectTransform RectTransform;
    public GameObject Root;
    public SUiElement Parent;
    
    public SUiElement(GameObject root)
    {
        Root = root;
        RectTransform = GetOrAdd<RectTransform>();
    }

    public void SetParent(Transform parent)
    {
        Root.transform.SetParent(parent, false);
    }

    public void SetParent(SUiElement parent)
    {
        Root.transform.SetParent(parent.Root.transform, false);
    }

    public void Remove()
    {
        Object.Destroy(Root);
    }

    protected T GetOrAdd<T>() where T : Component
    {
        var component = Root.GetComponent<T>();
        if (!component)
            component = Root.AddComponent<T>();
        return component;
    }
}

public class SUiElement<T> : SUiElement
{
    protected TMP_Text TextObject;
    
    protected Observable<bool> VisibilityObservable;

    public SUiElement(GameObject root) : base(root)
    { }
    
    protected virtual void VisibilityObservalbleChanged(bool value)
    {
        if(value == Root.activeSelf)
            return;
        
        Root.SetActive(value);
    }
    
    /// <summary>
    /// Binds the visibility of the container to an observable boolean value.
    /// </summary>
    /// <param name="observable">The observable boolean value to bind to.</param>
    public T BindVisibility(Observable<bool> observable)
    {
        UnbindVisibility();
        
        VisibilityObservable = observable;
        observable.OnValueChanged += VisibilityObservalbleChanged;
        VisibilityObservalbleChanged(observable.Value);
        
        return (T)(object)this;
    }
    
    /// <summary>
    /// Unbinds the visibility of the container from any previously bound observable.
    /// </summary>
    public T UnbindVisibility()
    {
        if (VisibilityObservable == null)
            return (T)(object)this;
        
        VisibilityObservable.OnValueChanged -= VisibilityObservalbleChanged;
        VisibilityObservable = null;

        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the text of the main text object
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public T Text(string text)
    {
        if (!TextObject)
            return (T)(object)this;
        
        TextObject.text = text;
        return (T)(object)this;
    }

    /// <summary>
    /// Set the text of the main text object as rich text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public T RichText(string text)
    {
        if (!TextObject)
            return (T)(object)this;

        TextObject.richText = true;
        TextObject.text = text;
        
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the font size of the main text object
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public T FontSize(int size)
    {
        if (!TextObject)
            return (T)(object)this;
        
        TextObject.fontSize = size;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Toggle auto sizing of the main text object
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    public T FontAutoSize(bool enabled)
    {
        if (!TextObject)
            return (T)(object)this;
        
        TextObject.enableAutoSizing = enabled;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the font color of the main text object
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public T FontColor(Color color)
    {
        if (!TextObject)
            return (T)(object)this;
        
        TextObject.color = color;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Finds a child object and returns it as a SUiElement
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="TObj"></typeparam>
    /// <returns></returns>
    public TObj Find<TObj> (string path) where TObj : SUiElement
    {
        return (TObj)Activator.CreateInstance(typeof(TObj), Root.transform.Find(path).gameObject);
    }

    /// <summary>
    /// Set the minimum offset of the rect transform
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public T MinOffset(int? x = null, int? y = null)
    {
        var offset = RectTransform.offsetMin;
        RectTransform.offsetMin = new Vector2(x ?? offset.x, y ?? offset.y);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the maximum offset of the rect transform
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public T MaxOffset(int? x = null, int? y = null)
    {
        var offset = RectTransform.offsetMax;
        RectTransform.offsetMax = new Vector2(x ?? offset.x, y ?? offset.y);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the size delta of the rect transform
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public T Size(float? width = null, float? height = null)
    {
        var size = RectTransform.sizeDelta;
        RectTransform.sizeDelta = new Vector2(width ?? size.x, height ?? size.y);
        return (T)(object)this;
    }

    /// <summary>
    /// Set the size delta of the rect transform
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public T Size(Vector2 size)
    {
        RectTransform.sizeDelta = size;
        return (T)(object)this;
    }

    /// <summary>
    /// Set the anchor position of the rect transform
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public T Position(float? x, float? y)
    {
        var anchoredPosition = RectTransform.anchoredPosition;
        RectTransform.anchoredPosition  = new Vector2(x ?? anchoredPosition.x, y ?? anchoredPosition.y);
        return (T)(object)this;
    }

    /// <summary>
    /// Set the size delta width of the rect transform
    /// </summary>
    /// <param name="width"></param>
    /// <returns></returns>
    public T Width(float width)
    {
        RectTransform.sizeDelta = new Vector2(width, RectTransform.sizeDelta.y);
        return (T)(object)this;
    }

    /// <summary>
    /// Set the size delta height of the rect transform
    /// </summary>
    /// <param name="height"></param>
    /// <returns></returns>
    public T Height(float height)
    {
        RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, height);
        return (T)(object)this;
    }

    /// <summary>
    /// Set the preferred height of the layout element
    /// </summary>
    /// <param name="height"></param>
    /// <returns></returns>
    public T PHeight(float height)
    {
        var layoutElement = GetOrAdd<LayoutElement>();
        layoutElement.preferredHeight = height;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the preferred width of the layout element
    /// </summary>
    /// <param name="width"></param>
    /// <returns></returns>
    public T PWidth(float width)
    {
        var layoutElement = GetOrAdd<LayoutElement>();
        layoutElement.preferredWidth = width;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the minimum height of the layout element
    /// </summary>
    /// <param name="height"></param>
    /// <returns></returns>
    public T MHeight(float height)
    {
        var layoutElement = GetOrAdd<LayoutElement>();
        layoutElement.minHeight = height;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the minimum width of the layout element
    /// </summary>
    /// <param name="width"></param>
    /// <returns></returns>
    public T MWidth(float width)
    {
        var layoutElement = GetOrAdd<LayoutElement>();
        layoutElement.minWidth = width;
        return (T)(object)this;
    }
    
    public T FlexHeight(float height)
    {
        var layoutElement = GetOrAdd<LayoutElement>();
        layoutElement.flexibleHeight = height;
        return (T)(object)this;
    }
    
    public T FlexWidth(float width)
    {
        var layoutElement = GetOrAdd<LayoutElement>();
        layoutElement.flexibleWidth = width;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the offsets of the rect transform
    /// </summary>
    /// <param name="top"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    /// <param name="left"></param>
    /// <returns></returns>
    public T RectPadding(float top, float right, float bottom, float left)
    {
        RectTransform.offsetMin = new Vector2(left, bottom);
        RectTransform.offsetMax = new Vector2(-right, -top);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the offsets of the rect transform
    /// </summary>
    /// <param name="rightLeft"></param>
    /// <param name="topBottom"></param>
    /// <returns></returns>
    public T RectPadding(float rightLeft, float topBottom)
    {
        RectTransform.offsetMin = new Vector2(rightLeft, topBottom);
        RectTransform.offsetMax = new Vector2(-rightLeft, -topBottom);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the offsets of the rect transform
    /// </summary>
    /// <param name="padding"></param>
    /// <returns></returns>
    public T RectPadding(float padding)
    {
        RectTransform.offsetMin = new Vector2(padding, padding);
        RectTransform.offsetMax = new Vector2(-padding, -padding);
        return (T)(object)this;
    }

    /// <summary>
    /// Sets the anchor and offsets to fill horizontally
    /// </summary>
    /// <returns></returns>
    public T HFill()
    {
        RectTransform.anchorMin = new Vector2(0, RectTransform.anchorMin.y);
        RectTransform.anchorMax = new Vector2(1, RectTransform.anchorMax.y);
        RectTransform.offsetMin = new Vector2(0, RectTransform.offsetMin.y);
        RectTransform.offsetMax = new Vector2(0, RectTransform.offsetMax.y);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Sets the anchor and offsets to fill vertically
    /// </summary>
    /// <returns></returns>
    public T VFill()
    {
        RectTransform.anchorMin = new Vector2(RectTransform.anchorMin.x, 0);
        RectTransform.anchorMax = new Vector2(RectTransform.anchorMax.x, 1);
        RectTransform.offsetMin = new Vector2(RectTransform.offsetMin.x, 0);
        RectTransform.offsetMax = new Vector2(RectTransform.offsetMax.x, 0);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the horizontal rect offset
    /// </summary>
    /// <param name="top"></param>
    /// <param name="bottom"></param>
    /// <returns></returns>
    public T HOffset(float top, float bottom)
    {
        RectTransform.offsetMin = new Vector2(RectTransform.offsetMin.x, bottom);
        RectTransform.offsetMax = new Vector2(RectTransform.offsetMax.x, -top);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the vertical rect offset
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public T VOffset(float left, float right)
    {
        RectTransform.offsetMin = new Vector2(left, RectTransform.offsetMin.y);
        RectTransform.offsetMax = new Vector2(-right, RectTransform.offsetMax.y);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the rect pivot
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public T Pivot(float? x = null, float? y = null)
    {
        var pivot = RectTransform.pivot;
        pivot = new Vector2(x ?? pivot.x, y ?? pivot.y);
        RectTransform.pivot = pivot;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Sets the aspect ratio mode for the objects's aspect ratio fitter.
    /// </summary>
    /// <param name="mode">The aspect ratio mode to apply.</param>
    public T AspectRatio(AspectRatioFitter.AspectMode mode)
    {
        var layout = GetOrAdd<AspectRatioFitter>();
        layout.aspectMode = mode;
        return (T)(object)this;
    }

    /// <summary>
    /// Set the rect anchor
    /// </summary>
    /// <param name="anchorType"></param>
    /// <returns></returns>
    public T Anchor(AnchorType anchorType)
    {
        if (RectTransform != null)
        {
            switch (anchorType)
            {
                case AnchorType.Fill:
                    RectTransform.anchorMin = Vector2.zero;
                    RectTransform.anchorMax = Vector2.one;
                    RectTransform.offsetMin = RectTransform.offsetMax = Vector2.zero;
                    break;

                case AnchorType.FillHorizontal:
                    RectTransform.anchorMin = new Vector2(0, RectTransform.anchorMin.y);
                    RectTransform.anchorMax = new Vector2(1, RectTransform.anchorMax.y);
                    RectTransform.offsetMin = new Vector2(0, RectTransform.offsetMin.y);
                    RectTransform.offsetMax = new Vector2(0, RectTransform.offsetMax.y);
                    break;

                case AnchorType.FillVertical:
                    RectTransform.anchorMin = new Vector2(RectTransform.anchorMin.x, 0);
                    RectTransform.anchorMax = new Vector2(RectTransform.anchorMax.x, 1);
                    RectTransform.offsetMin = new Vector2(RectTransform.offsetMin.x, 0);
                    RectTransform.offsetMax = new Vector2(RectTransform.offsetMax.x, 0);
                    break;

                case AnchorType.TopLeft:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0, 1);
                    break;

                case AnchorType.TopCenter:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0.5f, 1);
                    break;

                case AnchorType.TopRight:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(1, 1);
                    break;

                case AnchorType.MiddleLeft:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0, 0.5f);
                    break;

                case AnchorType.MiddleCenter:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    break;

                case AnchorType.MiddleRight:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(1, 0.5f);
                    break;

                case AnchorType.BottomLeft:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0, 0);
                    break;

                case AnchorType.BottomCenter:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0.5f, 0);
                    break;

                case AnchorType.BottomRight:
                    RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(1, 0);
                    break;
            }

            RectTransform.offsetMin = RectTransform.offsetMax = Vector2.zero;
        }

        return (T)(object)this;
    }
    
    public T Dock(EDockType type)
    {
        switch (type)
        {
            case EDockType.Top:
                Anchor(AnchorType.TopCenter);
                HFill();
                Pivot(0.5f, 1);
                break;
            case EDockType.Bottom:
                Anchor(AnchorType.BottomCenter);
                HFill();
                Pivot(0.5f, 0);
                break;
            case EDockType.Left:
                Anchor(AnchorType.MiddleLeft);
                VFill();
                Pivot(0, 0.5f);
                break;
            case EDockType.Right:
                Anchor(AnchorType.MiddleRight);
                VFill();
                Pivot(1, 0.5f);
                break;
            case EDockType.Center:
                Anchor(AnchorType.MiddleCenter);
                Pivot(0.5f, 0.5f);
                break;
            case EDockType.Fill:
                Anchor(AnchorType.Fill);
                break;
        }
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the visibility of the element (by setting the alpha and blocking raycasts)
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    public T Visible(bool visible)
    {
        var canvasGroup = GetOrAdd<CanvasGroup>();
        canvasGroup.alpha = visible ? 1 : 0;
        canvasGroup.blocksRaycasts = visible;
        return (T)(object)this;
    }
    
    /// <summary>
    /// Set the root gameobject to active or inactive
    /// </summary>
    /// <param name="active"></param>
    /// <returns></returns>
    public T Active(bool active)
    {
        Root.SetActive(active);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Toggle the root gameobject
    /// </summary>
    /// <returns></returns>
    public T Toggle()
    {
        Root.SetActive(!Root.activeSelf);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Fire an action when the container is clicked (FOR BUTTONS USE .Notify() INSTEAD)
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public T OnClick(Action action)
    {
        if (action == null)
            return (T)(object)this;
        
        var button = GetOrAdd<Button>();
        button.onClick.AddListener(action);
        return (T)(object)this;
    }
}

public class SUiElement<T, TData> : SUiElement<T>
{
    protected Observable<TData> Observable;
    private EObservableMode _observableMode;
    
    public SUiElement(GameObject root) : base(root)
    { }
    
    /// <summary>
    /// Bind an observable to the element value
    /// </summary>
    /// <param name="observable"></param>
    /// <returns></returns>
    public T Bind(Observable<TData> observable)
    {
        Unbind();
        
        Observable = observable;
        _observableMode = global::SUI.Observable.GetModeFromString("w");
        Observable.OnValueChanged += OnObservaleChanged;
        RegisterObservable(Observable);
        return (T)(object)this;
    }
    
    /// <summary>
    /// Unbind the current observable from the element value
    /// </summary>
    /// <returns></returns>
    public T Unbind()
    {
        if(Observable == null)
            return (T)(object)this;
        
        Observable.OnValueChanged -= OnObservaleChanged;
        UnregisterObservable(Observable);
        Observable = null;
        return (T)(object)this;
    }
    
    protected virtual void RegisterObservable(Observable<TData> observable)
    {
    }
    
    protected virtual void UnregisterObservable(Observable<TData> observable)
    {
    }
    
    protected virtual void OnObservaleChanged(TData value)
    { }
}