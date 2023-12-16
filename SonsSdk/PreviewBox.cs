using Shapes;
using UnityEngine;

namespace SonsSdk;

public class PreviewBox : IDisposable
{
    public GameObject Container;
    public Transform Transform;
    public Cuboid Cube;
    
    public bool IsActive { get; private set; }

    public PreviewBox()
    {
        Container = new GameObject("PreviewBox");
        Transform = Container.transform;
        Cube = Container.AddComponent<Cuboid>();

        IsActive = true;
    }

    public PreviewBox(Color color) : this()
    {
        SetColor(color);
    }
    
    public PreviewBox SetColor(Color color)
    {
        Cube.Color = color;
        return this;
    }

    public void Set(Vector3 pos, Vector3 size)
    {
        Transform.SetPositionAndRotation(pos, Quaternion.identity);
        Transform.localScale = size;
    }

    public void Set(Vector3 pos, Vector3 size, Quaternion rot)
    {
        Transform.SetPositionAndRotation(pos, rot);
        Transform.localScale = size;
    }
    
    public void SetActive(bool active)
    {
        if (active && !IsActive)
        {
            Container.SetActive(true);
            IsActive = true;
        }
        else if (!active && IsActive)
        {
            Container.SetActive(false);
            IsActive = false;
        }
    }

    public void Dispose()
    {
        Container.TryDestroy();
    }
}