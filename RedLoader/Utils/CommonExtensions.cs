using UnityEngine;

namespace RedLoader.Utils;

public static class CommonExtensions
{
    public static bool IsVectorNearZero(this Vector3 vec)
    {
        return Mathf.Abs(vec.x) < 0.01f && Mathf.Abs(vec.y) < 0.01f && Mathf.Abs(vec.z) < 0.01f;
    }
    
    public static bool IsPositionNearZero(this Transform tr)
    {
        return IsVectorNearZero(tr.position);
    }
}