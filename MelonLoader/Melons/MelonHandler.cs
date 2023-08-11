using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader.Fixes;
using MelonLoader.Utils;

namespace MelonLoader
{
    public static class MelonHandler
    {
        internal static void Setup()
        {
            if (!Directory.Exists(MelonEnvironment.CoreModDirectory))
                Directory.CreateDirectory(MelonEnvironment.CoreModDirectory);
            
            if (!Directory.Exists(MelonEnvironment.PluginsDirectory))
                Directory.CreateDirectory(MelonEnvironment.PluginsDirectory);
            
            if (!Directory.Exists(MelonEnvironment.ModsDirectory))
                Directory.CreateDirectory(MelonEnvironment.ModsDirectory);
        }

        private static bool firstSpacer = false;
        public static void LoadMelonsFromDirectory<T>(string path) where T : MelonTypeBase<T>
        {
            path = Path.GetFullPath(path);

            var loadingMsg = $"Loading {MelonTypeBase<T>.TypeName}s from '{path}'...";
            MelonLogger.WriteSpacer();
            MelonLogger.Msg(loadingMsg);

            bool hasWroteLine = false;

            var files = Directory.GetFiles(path, "*.dll");
            var melonAssemblies = new List<MelonAssembly>();
            foreach (var f in files)
            {
                var newFile = OldMelonFixer.Fix(f);
                
                if (!hasWroteLine)
                {
                    hasWroteLine = true;
                    MelonLogger.WriteLine(Color.Magenta);
                }

                var asm = MelonAssembly.LoadMelonAssembly(newFile, false);
                if (asm == null)
                    continue;

                melonAssemblies.Add(asm);
            }

            var melons = new List<T>();
            foreach (var asm in melonAssemblies)
            {
                asm.LoadMelons();
                foreach (var m in asm.LoadedMelons)
                {
                    if (m is T t)
                    {
                        melons.Add(t);
                    }
                    else
                    {
                        MelonLogger.Warning($"Failed to load Melon '{m.Info.Name}' from '{path}': The given Melon is a {m.MelonTypeName} and cannot be loaded as a {MelonTypeBase<T>.TypeName}. Make sure it's in the right folder.");
                        continue;
                    }
                }
            }

            if (hasWroteLine)
                MelonLogger.WriteSpacer();

            MelonBase.RegisterSorted(melons);

            if (hasWroteLine)
                MelonLogger.WriteLine(Color.Magenta);

            var count = MelonTypeBase<T>._registeredMelons.Count;
            MelonLogger.Msg($"{count} {MelonTypeBase<T>.TypeName.MakePlural(count)} loaded.");
            if (firstSpacer || (typeof(T) ==  typeof(MelonMod)))
                MelonLogger.WriteSpacer();
            firstSpacer = true;
        }
        
        public static void LoadModsFromDirectory(string path, string name)
        {
            path = Path.GetFullPath(path);

            var loadingMsg = $"Loading {name}s from '{path}'...";
            MelonLogger.WriteSpacer();
            MelonLogger.Msg(loadingMsg);

            bool hasWroteLine = false;

            var files = Directory.GetFiles(path, "*.dll");
            var melonAssemblies = new List<MelonAssembly>();
            foreach (var f in files)
            {
                var newFile = OldMelonFixer.Fix(f);
                
                if (!hasWroteLine)
                {
                    hasWroteLine = true;
                    MelonLogger.WriteLine(Color.Magenta);
                }

                var asm = MelonAssembly.LoadMelonAssembly(newFile, false);
                if (asm == null)
                    continue;

                melonAssemblies.Add(asm);
            }

            var melons = new List<MelonBase>();
            foreach (var asm in melonAssemblies)
            {
                asm.LoadMelons();
                foreach (var m in asm.LoadedMelons)
                {
                    if (m is not MelonPlugin)
                    {
                        melons.Add(m);
                    }
                }
            }

            if (hasWroteLine)
                MelonLogger.WriteSpacer();

            MelonBase.RegisterSorted(melons);

            if (hasWroteLine)
                MelonLogger.WriteLine(Color.Magenta);

            var count = MelonBase._registeredMelons.Count;
            MelonLogger.Msg($"{count} {name.MakePlural(count)} loaded.");
            MelonLogger.WriteSpacer();
            firstSpacer = true;
        }
    }
}
