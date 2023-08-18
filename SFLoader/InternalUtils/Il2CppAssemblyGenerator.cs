using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SFLoader.Modules;
using SFLoader.Utils;

namespace SFLoader.InternalUtils
{
    internal static class Il2CppAssemblyGenerator
    {
        public static readonly MelonModule.Info moduleInfo = new MelonModule.Info(
            $"{MelonEnvironment.LoaderFolderName}{Path.DirectorySeparatorChar}Dependencies{Path.DirectorySeparatorChar}Il2CppAssemblyGenerator{Path.DirectorySeparatorChar}Il2CppAssemblyGenerator.dll"
            , () => !MelonUtils.IsGameIl2Cpp());

        internal static bool Run()
        {
            var module = MelonModule.Load(moduleInfo);
            if (module == null)
                return true;
            
            MelonLogger.MsgDirect("Loading Il2CppAssemblyGenerator...");
            if (MelonUtils.IsWindows)
            {
                IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
                BootstrapInterop.DisableCloseButton(windowHandle);
            }
            
            var ret = module.SendMessage("Run");
            
            if (MelonUtils.IsWindows)
            {
                IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
                BootstrapInterop.EnableCloseButton(windowHandle);
            }
            return ret is 0;
        }
    }
}