using Il2CppInterop.Runtime.Injection;
using RedLoader;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SonsSdk;

public static class SonsInteractionTools
{
    public static T CreateInteraction<T>(Transform parent, float radius = 0.1f) where T : Component
    {
        var gameObject = new GameObject("InteractionProxy");
        gameObject.transform.SetParent(parent);
        gameObject.layer = LayerMask.NameToLayer("PickUp");
        var collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = radius;
        collider.isTrigger = true;
        return gameObject.AddComponent<T>();
    }
}