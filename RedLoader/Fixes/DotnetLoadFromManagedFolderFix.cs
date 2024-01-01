
using RedLoader.Utils;
#if NET6_0
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
#endif

namespace RedLoader.Fixes
{
    internal static class DotnetLoadFromManagedFolderFix
    {

#if !NET6_0
        internal static void Install()
        {

        }
#else
        //TODO Update for non-windows platforms in future, or when updating runtime
        private static readonly string OurRuntimeDir = Path.Combine(LoaderEnvironment.OurRuntimeDirectory, "runtimes", "win", "lib", "net6.0"); 

        internal static void Install()
        {
            AssemblyLoadContext.Default.Resolving += OnResolve;
        }

        private static Assembly TryLoad(AssemblyLoadContext alc, string path)
        {
            if (File.Exists(path))
            {
                //MelonDebug.Msg($"[DotnetManagedFolder] Loading from {path}...");
                return alc.LoadFromAssemblyPath(path);
            }

            return null;
        }

        private static Assembly OnResolve(AssemblyLoadContext alc, AssemblyName name)
        {
            if (name.Name == "RedLoader.ModHandler")
                //Redirect ModHandler to main RedLoader dll (us)
                return Assembly.GetExecutingAssembly();
            
            var filename = name.Name + ".dll";
            //MelonLogger.Msg($"Resolving: {filename}");
            
            //filename = filename.Replace("Il2CppSons.", "Sons.").Replace("Il2CppTheForest", "TheForest");
            //MelonLogger.Msg($"Resolving NewName: {filename}");

            var osSpecificPath = Path.Combine(OurRuntimeDir, filename);
            var il2cppPath = Path.Combine(LoaderEnvironment.Il2CppAssembliesDirectory, filename);
            //var managedPath = Path.Combine(MelonEnvironment.MelonManagedDirectory, filename);
            var modsPath = Path.Combine(LoaderEnvironment.ModsDirectory, filename);
            var userlibsPath = Path.Combine(LoaderEnvironment.LibsDirectory, filename);
            var gameRootPath = Path.Combine(LoaderEnvironment.GameRootDirectory, filename);

            var ret = TryLoad(alc, osSpecificPath)
                      ?? TryLoad(alc, il2cppPath)
                      //?? TryLoad(alc, managedPath)
                      ?? TryLoad(alc, modsPath)
                      ?? TryLoad(alc, userlibsPath)
                      ?? TryLoad(alc, gameRootPath);

            if (ret == null)
                MelonDebug.Error($"[DotnetManagedFolder] Failed to find {filename} in any of the known search directories");

            return ret;
        }
#endif
    }
}