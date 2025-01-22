using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RedLoader.Preloader.Core;
using RedLoader;
using RedLoader.Utils;

namespace RedLoader.Unity.IL2CPP;

internal static class UnityPreloaderRunner
{
    public static void PreloaderMain()
    {
        var bepinPath =
            Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(EnvVars.DOORSTOP_INVOKE_DLL_PATH)));

        PlatformUtils.SetPlatform();

        LoaderEnvironment.SetExecutablePath(EnvVars.DOORSTOP_PROCESS_PATH, EnvVars.DOORSTOP_DLL_SEARCH_DIRS);
        
        CorePreferences.Init();

        // Cecil 0.11 requires one to manually set up list of trusted assemblies for assembly resolving
        // The main BCL path
        // AppDomain.CurrentDomain.AddCecilPlatformAssemblies(LoaderEnvironment.ManagedPath);
        // The parent path -> .NET has some extra managed DLLs in there
        // AppDomain.CurrentDomain.AddCecilPlatformAssemblies(Path.GetDirectoryName(LoaderEnvironment.ManagedPath));
        AppDomain.CurrentDomain.AddCecilPlatformAssemblies(Path.Combine(LoaderEnvironment.LoaderDirectory, "dotnet"));

        AppDomain.CurrentDomain.AssemblyResolve += LocalResolve;

        Preloader.Run();
    }

    internal static Assembly LocalResolve(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        var foundAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                     .FirstOrDefault(x => x.GetName().Name == assemblyName.Name);

        if (foundAssembly != null)
            return foundAssembly;

        if (Utility.TryResolveDllAssembly(assemblyName, LoaderEnvironment.LoaderAssemblyDirectory, out foundAssembly)
           || Utility.TryResolveDllAssembly(assemblyName, LoaderEnvironment.LibsDirectory, out foundAssembly)
         || Utility.TryResolveDllAssembly(assemblyName, LoaderEnvironment.PatcherPluginPath, out foundAssembly)
         || Utility.TryResolveDllAssembly(assemblyName, LoaderEnvironment.ModsDirectory, out foundAssembly))
            return foundAssembly;

        return null;
    }
}
