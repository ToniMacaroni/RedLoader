using System.Text;
using System.Text.RegularExpressions;
using RedLoader.Utils;

namespace SonsSdk;

internal static class AssetMapCreator
{
    public static void CreateAssetMap(PathObject path, string name, IEnumerable<string> values)
    {
        var result = CreateAssetMap(name, values);
        (path / $"{name}AssetMap.cs").WriteText(result);
    }
    
    public static string CreateAssetMap(string name, IEnumerable<string> values)
    {
        var mapping = new Dictionary<string, string>();
        foreach (var value in values)
        {
            if(string.IsNullOrEmpty(value))
                continue;
            
            mapping[value] = CodeWriter.SanitizeVariableName(value);
        }
        
        var sb = new StringBuilder();
        sb.AppendLine($"// Auto-generated");
        sb.AppendLine($"// ReSharper disable All");
        
        sb.AppendLine($"namespace SonsSdk;");
        sb.AppendLine();
        sb.AppendLine($"public static class {name}AssetMap");
        sb.AppendLine("{");
        foreach (var (key, value) in mapping)
        {
            sb.AppendLine($"    public const string {value} = \"{key}\";");
        }
        sb.AppendLine("}");
        
        return sb.ToString();
    }
}