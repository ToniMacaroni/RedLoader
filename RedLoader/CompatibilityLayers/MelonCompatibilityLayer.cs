using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RedLoader.Modules;

namespace RedLoader
{
    public static class MelonCompatibilityLayer
    {
        public static string baseDirectory = $"MelonLoader{Path.DirectorySeparatorChar}Dependencies{Path.DirectorySeparatorChar}CompatibilityLayers";

        private static List<MelonModule.Info> layers = new List<MelonModule.Info>()
        {
            // Il2Cpp Unity Tls - No longer needed in CoreCLR
            // new MelonModule.Info(Path.Combine(baseDirectory, "Il2CppUnityTls.dll"), () => !MelonUtils.IsGameIl2Cpp()),

            // Illusion Plugin Architecture
            new MelonModule.Info(Path.Combine(baseDirectory, "IPA.dll"), LoaderUtils.IsGameIl2Cpp),
            new MelonModule.Info(Path.Combine(baseDirectory, "EOS.dll"), () => !LoaderUtils.IsWindows)
        };
        
        private static void CheckGameLayerWithPlatform(string name, Func<bool> shouldBeIgnored)
        {
            string nameNoSpaces = name.Replace(' ', '_');
            foreach (var file in Directory.GetFiles(baseDirectory))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.StartsWith(nameNoSpaces))
                    layers.Add(new MelonModule.Info(file, shouldBeIgnored));
            }
        }

        private static void CheckGameLayer(string name)
        {
            CheckGameLayerWithPlatform(name, () => false);
            CheckGameLayerWithPlatform($"{name}_Mono", () => LoaderUtils.IsGameIl2Cpp());
            CheckGameLayerWithPlatform($"{name}_Il2Cpp", () => !LoaderUtils.IsGameIl2Cpp());
        }

        internal static void LoadModules()
        {
            if (!Directory.Exists(baseDirectory))
                return;

            CheckGameLayer(InternalUtils.UnityInformationHandler.GameName);
            CheckGameLayer(InternalUtils.UnityInformationHandler.GameDeveloper);
            CheckGameLayer($"{InternalUtils.UnityInformationHandler.GameDeveloper}_{InternalUtils.UnityInformationHandler.GameName}");

            foreach (var m in layers)
                MelonModule.Load(m);

            foreach (var file in Directory.GetFiles(baseDirectory))
            {
                string fileName = Path.GetFileName(file);
                if (layers.Find(x => Path.GetFileName(x.fullPath).Equals(fileName)) == null)
                    File.Delete(file);
            }
        }
    }
}