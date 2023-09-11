using System.Text;
using System.Text.RegularExpressions;
using RedLoader.Utils;

namespace SonsSdk;

internal static class AssetMapCreator
{
    private static readonly Regex _regex = new(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);
    
    private static string SanitizeVariableName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        string sanitized = input.Replace(' ', '_');

        if (char.IsDigit(sanitized[0]))
            sanitized = "_" + sanitized;

        sanitized = _regex.Replace(sanitized, string.Empty);

        return sanitized;
    }

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
            
            mapping[value] = SanitizeVariableName(value);
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