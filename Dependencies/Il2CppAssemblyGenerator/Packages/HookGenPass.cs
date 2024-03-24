using System;
using Il2CppInterop.Generator.Contexts;
using Il2CppInterop.Generator.Passes;
using RedLoader.Utils;

namespace RedLoader.Il2CppAssemblyGenerator.Packages;

internal class HookGenPass : ICustomPass
{
    public void DoPass(RewriteGlobalContext context)
    {
        //var depsDirs = new List<string> { LoaderEnvironment.Il2CppAssembliesDirectory, Path.Combine(LoaderEnvironment.LoaderDirectory, "net6") };
        // foreach (var assembly in assemblies)
        // {
        //     GenerateHookAssembly(Path.Combine(LoaderEnvironment.Il2CppAssembliesDirectory, assembly + ".dll"), 
        //         Path.Combine(LoaderEnvironment.HooksDirectory, "HK_" + assembly + ".dll"), depsDirs);
        // }
    
        foreach (var assembly in context.Assemblies)
        {
            var name = assembly.OriginalAssembly.Name.Name;
            if(!name.Contains("Sons") && !name.Contains("Endnight"))
                continue;
                
            RLog.Msg($"Generating hooks for {assembly.OriginalAssembly.Name.Name}");
            var gen = new HookGeneratorV2(assembly.OriginalAssembly.MainModule, assembly.NewAssembly.MainModule);
                
            try
            {
                gen.Generate();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while processing {name}: \n {e}");
            }
        }
    }
}