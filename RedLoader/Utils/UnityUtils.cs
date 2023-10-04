using System.Collections.Generic;

namespace RedLoader.Utils;

public static class UnityUtils
{
    public static Dictionary<string, T> CreateAssetMap<T>(this IEnumerable<T> values) where T : UnityEngine.Object
    {
        var mapping = new Dictionary<string, T>();
        foreach (var value in values)
        {
            if(value == null)
                continue;
            
            mapping[value.name] = value;
        }

        return mapping;
    }
}