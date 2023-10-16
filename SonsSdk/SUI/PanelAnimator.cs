using Il2CppInterop.Runtime.Startup;
using Il2CppSystem;
using RedLoader;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SUI;

public class PanelPosAnimator
{
    public Vector3? StartPos;
    public Vector3 Distance;
    public float Time;

    public PanelPosAnimator(Vector3 distance, float time = 0.5f)
    {
        Distance = distance;
        Time = time;
    }

    public PanelPosAnimator(float? x = null, float? y = null, float? z = null, float time = 0.5f)
    {
        Distance = new(x ?? 0, y ?? 0, z ?? 0);
        Time = time;
    }

    public void AnimIn(SContainerOptions container)
    {
        container.Active(true);
        StartPos ??= container.RectTransform.localPosition;
        container.RectTransform.localPosition = StartPos.Value - Distance;

        if (container.Root.GetComponent<iTween>())
            return;
        
        iTween.MoveTo(container.Root, container.RectTransform.TransformPoint(Distance), Time);
    }
    
    public void AnimOut(SContainerOptions container)
    {
        iTween.Stop(container.Root);
        container.Active(false);
    }
    
    public static PanelPosAnimator FromBottom = new(new Vector3(0, 300, 0));
    public static PanelPosAnimator FromTop = new(new Vector3(0, -300, 0));
    public static PanelPosAnimator FromLeft = new(new Vector3(300, 0, 0));
    public static PanelPosAnimator FromRight = new(new Vector3(-300, 0, 0));
}

public class PanelSizeAnimator
{
    public Vector2? StartSize;
    public Vector2 Size;
    public float Time;

    public PanelSizeAnimator(Vector2 size, float time = 0.5f)
    {
        Size = size;
        Time = time;
    }

    public void AnimIn(SContainerOptions container)
    {
        container.Active(true);
        StartSize ??= container.RectTransform.sizeDelta;
        container.RectTransform.sizeDelta = StartSize.Value - Size;

        if (container.Root.GetComponent<iTween>())
            return;
        
        iTween.ScaleBy(container.Root, Size, Time);
    }
    
    public void AnimOut(SContainerOptions container)
    {
        iTween.Stop(container.Root);
        container.Active(false);
    }
}