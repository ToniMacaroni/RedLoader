
using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using RedLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SUI;

using static UnityEngine.Mathf;

public class ColorPicker : MonoBehaviour
{
    private const float Recip2Pi = 0.159154943f;
    private const string ColorPickerShaderName = "UI/ColorPicker";

    private static readonly int PropIdHSV             = Shader.PropertyToID("_HSV");
    private static readonly int PropIdAspectRatio     = Shader.PropertyToID("_AspectRatio");
    private static readonly int PropIdHueCircleInner  = Shader.PropertyToID("_HueCircleInner");
    private static readonly int PropIdSvSquareSize    = Shader.PropertyToID("_SVSquareSize");

    internal static Shader ColorPickerShaderValue;
    private Material _material;

    private enum PointerDownLocation { HueCircle, SvSquare, Outside }
    private PointerDownLocation _pointerDownLocation = PointerDownLocation.Outside;

    private RectTransform _rectTransform;
    private Image _image;

    float _h, _s, _v;

    public Color Color
    {
        get { return Color.HSVToRGB(_h, _s, _v); }
        set {
            Color.RGBToHSV(value, out _h, out _s, out _v);
            ApplyColor();
        }
    }

    public event Action<Color> OnColorChanged;

    public void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();

        _h = _s = _v = 0;

        _image.material = Instantiate(_image.material);
        _material = _image.materialForRendering;

        ApplyColor();
    }

    public void OnEnable()
    {
        _material = _image.materialForRendering;
        ApplyColor(false);
    }

    private void Update()
    {
        if (!_material) return;

        var rect = _rectTransform.rect;

        _material.SetFloat(PropIdAspectRatio, rect.width / rect.height);
    }

    public void OnDrag(BaseEventData eventData)
    {
        if (!_material) return;

        var pos = GetRelativePosition(new PointerEventData(eventData.Pointer));

        if (_pointerDownLocation == PointerDownLocation.HueCircle)
        {
            _h = (Atan2(pos.y, pos.x) * Recip2Pi + 1) % 1;
            ApplyColor();
        }

        if (_pointerDownLocation == PointerDownLocation.SvSquare)
        {
            var size = _material.GetFloat(PropIdSvSquareSize);

            _s = InverseLerp(-size, size, pos.x);
            _v = InverseLerp(-size, size, pos.y);
            ApplyColor();
        }
    }

    public void OnPointerDown(BaseEventData eventData)
    {
        if (!_material) return;

        var pos = GetRelativePosition(new PointerEventData(eventData.Pointer));

        var r = pos.magnitude;

        if (r < .5f && r > _material.GetFloat(PropIdHueCircleInner))
        {
            _pointerDownLocation = PointerDownLocation.HueCircle;
            _h = (Atan2(pos.y, pos.x) * Recip2Pi + 1) % 1;
            ApplyColor();
        }
        else
        {
            var size = _material.GetFloat(PropIdSvSquareSize);

            // s -> x, v -> y
            if (pos.x >= -size && pos.x <= size && pos.y >= -size && pos.y <= size)
            {
                _pointerDownLocation = PointerDownLocation.SvSquare;
                _s = InverseLerp(-size, size, pos.x);
                _v = InverseLerp(-size, size, pos.y);
                ApplyColor();
            }
        }
    }

    public void OnPointerUp(BaseEventData eventData)
    {
        _pointerDownLocation = PointerDownLocation.Outside;
    }

    private void ApplyColor(bool invokeCallback = true)
    {
        _material.SetVector(PropIdHSV, new Vector3(_h, _s, _v));

        if(invokeCallback)
            OnColorChanged?.Invoke(Color);
    }

    public void OnDestroy()
    {
        if (_material != null)
            DestroyImmediate(_material);
    }

    // Returns position in range -0.5..0.5 when it's inside color picker square area
    public Vector2 GetRelativePosition(PointerEventData eventData)
    {
        var rect = GetSquaredRect();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out var rtPos);

        return new Vector2(InverseLerpUnclamped(rect.xMin, rect.xMax, rtPos.x), InverseLerpUnclamped(rect.yMin, rect.yMax, rtPos.y)) - Vector2.one * 0.5f;
    }

    public Rect GetSquaredRect()
    {
        var rect = _rectTransform.rect;
        var smallestDimension = Min(rect.width, rect.height);
        return new Rect(rect.center - Vector2.one * smallestDimension * 0.5f, Vector2.one * smallestDimension);
    }

    public float InverseLerpUnclamped(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }
}
