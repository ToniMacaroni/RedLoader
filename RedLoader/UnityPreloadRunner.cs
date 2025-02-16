using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        
        InitReshade();
        
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

    internal static void InitReshade()
    {
        RLog.Msg("Trying to fix and init Reshade");
        
        var dxgidll = Path.Combine(LoaderEnvironment.GameRootDirectory, "dxgi.dll");
        var reshadedll = Path.Combine(LoaderEnvironment.GameRootDirectory, "reshade.dll");
        
        if (File.Exists(dxgidll))
        {
            File.Move(dxgidll, reshadedll);
            RLog.Msg("Renamed Reshade");
        }

        if (File.Exists(reshadedll))
        {
            NativeLibrary.TryLoad(reshadedll, out var handle);
            RLog.Msg($"Loaded Reshade: {handle != IntPtr.Zero}");
        }
    }
}
