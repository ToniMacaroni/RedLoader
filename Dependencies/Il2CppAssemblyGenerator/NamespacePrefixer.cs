using System.IO;
using RedLoader.Utils;
using Mono.Cecil;

namespace RedLoader.Il2CppAssemblyGenerator;

public class NamespacePrefixer
{
    public const string NAMESPACE_PREFIX = "Il2Cpp";

    public static bool TryProcess(string inputAssembly)
    {
        try
        {
            Process(inputAssembly);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
    
    public static void Process(string inputAssembly)
    {
        var filename = Path.GetFileName(inputAssembly);
        Core.Logger.Msg("Processing: " + filename);

        if (filename.Contains("Il2Cpp") || filename.StartsWith("Unity.") || filename.StartsWith("UnityEngine."))
        {
            Core.Logger.Msg("Skipping: " + filename);
            return;
        }

        if(!Directory.Exists(Path.Combine(MelonEnvironment.MelonLoaderDirectory, "Il2CppAssembliesPrefixed")))
        {
            Directory.CreateDirectory(Path.Combine(MelonEnvironment.MelonLoaderDirectory, "Il2CppAssembliesPrefixed"));
        }
        string outputAssemblyPath = Path.Combine(MelonEnvironment.MelonLoaderDirectory, "Il2CppAssembliesPrefixed", NAMESPACE_PREFIX + filename);

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.Combine(MelonEnvironment.MelonLoaderDirectory, "Il2CppAssemblies"));
        resolver.AddSearchDirectory(Path.Combine(MelonEnvironment.MelonLoaderDirectory, "Managed"));
        resolver.AddSearchDirectory(Path.Combine(MelonEnvironment.MelonLoaderDirectory, "net6"));
        resolver.AddSearchDirectory(MelonEnvironment.LibsDirectory);
        resolver.AddSearchDirectory(MelonEnvironment.ModsDirectory);

        var readerParameters = new ReaderParameters
        {
            AssemblyResolver = resolver
        };

        using var assembly = AssemblyDefinition.ReadAssembly(inputAssembly, readerParameters);
        
        foreach (var module in assembly.Modules)
        {
            foreach (var type in module.Types)
            {
                PrefixNamespace(type, NAMESPACE_PREFIX);
            }
        }
        
        assembly.Name.Name = NAMESPACE_PREFIX + assembly.Name.Name;
        assembly.Write(outputAssemblyPath);
    }

    static void PrefixNamespace(TypeDefinition type, string prefix)
    {
        if (!string.IsNullOrEmpty(type.Namespace))
        {
            type.Namespace = prefix + type.Namespace;
        }

        foreach (var nestedType in type.NestedTypes)
        {
            PrefixNamespace(nestedType, prefix);
        }
    }
}