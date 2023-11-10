using Endnight.Utilities;
using Il2CppInterop.Runtime.Injection;
using RedLoader;
using UnityEngine;

namespace SonsSdk;

public class FollowMouseHandler : MonoBehaviour
{
    private RectTransform _rect;
    public Vector2 Offset = new();

    private Vector2 _multiplier;
    
    static FollowMouseHandler()
    {
        ClassInjector.RegisterTypeInIl2Cpp<FollowMouseHandler>();
    }

    private void Awake()
    {
        _multiplier = new(1920f / Screen.width, 1080f / Screen.height);
        _rect = gameObject.GetOrAddComponent<RectTransform>();
    }

    private void Update()
    {
        var mousePos = Input.mousePosition;
        _rect.anchoredPosition = new Vector2((mousePos.x + Offset.x) * _multiplier.x, (mousePos.y + Offset.y) * _multiplier.y);
    }
}