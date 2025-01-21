using System.Diagnostics;
using System.Reflection;
using Alt.Json;
using RedLoader;
using RedLoader.Utils;
using SonsSdk;

namespace SonsLoaderPlugin;

public class ManifestReader
{
    public static ManifestData TryReadManifest(Assembly assembly)
    {
        var manifestName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(".manifest.json"));
        if (manifestName == null)
            return null;

        var manifest = assembly.GetManifestResourceStream(manifestName);
        if (manifest == null)
            return null;
        
        using var reader = new StreamReader(manifest);
        var json = reader.ReadToEnd();
        // var data = JSON.Load(json).Make<ManifestData>();
        var data = JsonConvert.DeserializeObject<ManifestData>(json);
        //PrintManifest(assembly, data);
        return data;
    }
    
    public static ManifestData TryReadManifest(string path)
    {
        if (!File.Exists(path))
            return null;
        
        var data = JsonConvert.DeserializeObject<ManifestData>(File.ReadAllText(path));
        //PrintManifest(assembly, data);
        return data;
    }

    private static void PrintManifest(Assembly assembly, ManifestData data)
    {
        RLog.Msg(ConsoleColor.Magenta, $"Manifest for [{assembly.GetName().Name}]:");
        foreach (var prop in typeof(ManifestData).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            RLog.Msg(ConsoleColor.Magenta, $"{prop.Name}: {prop.GetValue(data)}");
        }
    }
}
