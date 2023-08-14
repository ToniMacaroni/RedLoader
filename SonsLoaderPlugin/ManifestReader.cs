using System.Diagnostics;
using System.Reflection;
using MelonLoader;
using MelonLoader.TinyJSON;
using SonsSdk;

namespace SonsLoaderPlugin;

public class ManifestReader
{
    public static ManifestData TryReadManifest(Assembly assembly)
    {
        var manifest = assembly.GetManifestResourceStream(assembly.GetName().Name + ".manifest.json");
        if (manifest == null)
            return null;
        
        using var reader = new StreamReader(manifest);
        var json = reader.ReadToEnd();
        var data = JSON.Load(json).Make<ManifestData>();
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