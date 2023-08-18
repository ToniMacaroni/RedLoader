using System.Diagnostics;
using System.Reflection;
using SFLoader;
using SFLoader.TinyJSON;
using SFLoader.Utils;
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
        var data = JSON.Load(json).Make<ManifestData>();
        //PrintManifest(assembly, data);
        return data;
    }
    
    public static ManifestData TryReadManifest(PathObject path)
    {
        if (!path.FileExists())
            return null;
        
        var data = path.ReadJson<ManifestData>();
        //PrintManifest(assembly, data);
        return data;
    }

    private static void PrintManifest(Assembly assembly, ManifestData data)
    {
        MelonLogger.Msg(ConsoleColor.Magenta, $"Manifest for [{assembly.GetName().Name}]:");
        foreach (var prop in typeof(ManifestData).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            MelonLogger.Msg(ConsoleColor.Magenta, $"{prop.Name}: {prop.GetValue(data)}");
        }
    }
}