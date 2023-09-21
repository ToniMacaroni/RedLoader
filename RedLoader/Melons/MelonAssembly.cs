using Semver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using RedLoader.Utils;
using RedLoader.Fixes;
#if NET6_0
using System.Runtime.Loader;
#endif

namespace RedLoader
{
    public sealed class MelonAssembly
    {
        #region Static

        /// <summary>
        /// Called before a process of resolving Melons from a MelonAssembly has started.
        /// </summary>
        public static readonly MelonEvent<Assembly> OnAssemblyResolving = new();

        public static event LemonFunc<MelonAssembly, ResolvedMelons> CustomMelonResolvers;

        internal static List<MelonAssembly> loadedAssemblies = new();

        /// <summary>
        /// List of all loaded MelonAssemblies.
        /// </summary>
        public static ReadOnlyCollection<MelonAssembly> LoadedAssemblies => loadedAssemblies.AsReadOnly();

        /// <summary>
        /// Tries to find the instance of Melon with type T, whether it's registered or not
        /// </summary>
        public static T FindMelonInstance<T>() where T : ModBase
        {
            foreach (var asm in loadedAssemblies)
            {
                foreach (var melon in asm.loadedMelons)
                {
                    if (melon is T teaMelon)
                        return teaMelon;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the MelonAssembly of the given member. If the given member is not in any MelonAssembly, returns null.
        /// </summary>
        public static MelonAssembly GetMelonAssemblyOfMember(MemberInfo member, object obj = null)
        {
            if (member == null)
                return null;

            if (obj != null && obj is ModBase melon)
                return melon.ModAssembly;

            var name = member.DeclaringType.Assembly.FullName;
            var ma = loadedAssemblies.Find(x => x.Assembly.FullName == name);
            return ma;
        }

        /// <summary>
        /// Loads or finds a MelonAssembly from path.
        /// </summary>
        /// <param name="path">Path of the MelonAssembly</param>
        /// <param name="loadMelons">Sets whether Melons should be auto-loaded or not</param>
        public static MelonAssembly LoadMelonAssembly(string path, bool loadMelons = true)
        {
            if (path == null)
            {
                RLog.Error("Failed to load a Melon Assembly: Path cannot be null.");
                return null;
            }

            path = Path.GetFullPath(path);

            try
            {
#if NET6_0
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
#else
                var assembly = Assembly.LoadFrom(path);
#endif
                return LoadMelonAssembly(path, assembly, loadMelons);
            }
            catch (Exception ex)
            {
                RLog.Error($"Failed to load Melon Assembly from '{path}':\n{ex}");
                return null;
            }
        }

        /// <summary>
        /// Loads or finds a MelonAssembly from raw Assembly Data.
        /// </summary>
        public static MelonAssembly LoadRawMelonAssembly(string path, byte[] assemblyData, byte[] symbolsData = null, bool loadMelons = true)
        {
            if (assemblyData == null)
            {
                RLog.Error("Failed to load a Melon Assembly: assemblyData cannot be null.");
                return null;
            }

            try
            {
#if NET6_0
                var fileStream = new MemoryStream(assemblyData);
                var symStream = symbolsData == null ? null : new MemoryStream(symbolsData);

                var assembly = AssemblyLoadContext.Default.LoadFromStream(fileStream, symStream);
#else
                var assembly = symbolsData != null ? Assembly.Load(assemblyData, symbolsData) : Assembly.Load(assemblyData);
#endif
                return LoadMelonAssembly(path, assembly, loadMelons);
            }
            catch (Exception ex)
            {
                RLog.Error($"Failed to load Melon Assembly from raw Assembly Data (length {assemblyData.Length}):\n{ex}");
                return null;
            }
        }

        /// <summary>
        /// Loads or finds a MelonAssembly.
        /// </summary>
        public static MelonAssembly LoadMelonAssembly(string path, Assembly assembly, bool loadMelons = true)
        {
            if (!File.Exists(path))
                path = assembly.Location;

            if (assembly == null)
            {
                RLog.Error("Failed to load a Melon Assembly: Assembly cannot be null.");
                return null;
            }

            var ma = loadedAssemblies.Find(x => x.Assembly.FullName == assembly.FullName);
            if (ma != null)
                return ma;

            var shortPath = path;
            if (shortPath.StartsWith(LoaderEnvironment.GameRootDirectory))
                shortPath = "." + shortPath.Remove(0, LoaderEnvironment.GameRootDirectory.Length);

            OnAssemblyResolving.Invoke(assembly);
            ma = new MelonAssembly(assembly, path);
            loadedAssemblies.Add(ma);

            if (loadMelons)
                ma.LoadMelons();

            RLog.MsgDirect(Color.DarkGray, $"Melon Assembly loaded: '{shortPath}'");
            RLog.MsgDirect(Color.DarkGray, $"SHA256 Hash: '{ma.Hash}'");
            return ma;
        }

        #endregion

        #region Instance

        private bool melonsLoaded;

        private readonly List<ModBase> loadedMelons = new();
        private readonly List<RottenMelon> rottenMelons = new();

        public readonly MelonEvent OnUnregister = new();

        /// <summary>
        /// A SHA256 Hash of the Assembly.
        /// </summary>
        public string Hash { get; private set; }

        public Assembly Assembly { get; private set; }

        public string Location { get; private set; }

        /// <summary>
        /// A list of all loaded Melons in the Assembly.
        /// </summary>
        public ReadOnlyCollection<ModBase> LoadedMelons => loadedMelons.AsReadOnly();

        /// <summary>
        /// A list of all broken Melons in the Assembly.
        /// </summary>
        public ReadOnlyCollection<RottenMelon> RottenMelons => rottenMelons.AsReadOnly();

        private MelonAssembly(Assembly assembly, string location)
        {
            Assembly = assembly;
            Location = location ?? "";
            Hash = LoaderUtils.ComputeSimpleSHA256Hash(Location);
        }

        /// <summary>
        /// Unregisters all Melons in this Assembly.
        /// </summary>
        public void UnregisterMelons(string reason = null, bool silent = false)
        {
            foreach (var m in loadedMelons)
                m.UnregisterInstance(reason, silent);

            OnUnregister.Invoke();
        }

        private void OnApplicationQuit()
        {
            UnregisterMelons("RedLoader is deinitializing.", true);
        }

        public void LoadMelons()
        {
            if (melonsLoaded)
                return;

            melonsLoaded = true;

            GlobalEvents.OnApplicationDefiniteQuit.Subscribe(OnApplicationQuit);

            // \/ Custom Resolver \/
            var resolvers = CustomMelonResolvers?.GetInvocationList();
            if (resolvers != null)
            {
                foreach (LemonFunc<MelonAssembly, ResolvedMelons> r in resolvers)
                {
                    var customMelon = r.Invoke(this);

                    loadedMelons.AddRange(customMelon.loadedMelons);
                    rottenMelons.AddRange(customMelon.rottenMelons);
                }
            }

            // \/ Default resolver \/
            var info = LoaderUtils.PullAttributeFromAssembly<MelonInfoAttribute>(Assembly);
            if (info != null && info.SystemType != null && info.SystemType.IsSubclassOf(typeof(ModBase)))
            {
                if (info.SystemType.IsSubclassOf(typeof(LoaderPlugin)))
                {
                    ModBase mod;
                    try
                    {
                        mod = (ModBase)Activator.CreateInstance(info.SystemType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
                    }
                    catch (Exception ex)
                    {
                        mod = null;
                        rottenMelons.Add(new RottenMelon(info.SystemType, "Failed to create an instance of the Melon.", ex));
                    }

                    if (mod != null)
                    {
                        var priorityAttr = LoaderUtils.PullAttributeFromAssembly<MelonPriorityAttribute>(Assembly);
                        var colorAttr = LoaderUtils.PullAttributeFromAssembly<MelonColorAttribute>(Assembly);
                        var authorColorAttr = LoaderUtils.PullAttributeFromAssembly<MelonAuthorColorAttribute>(Assembly);
                        var additionalCreditsAttr = LoaderUtils.PullAttributeFromAssembly<MelonAdditionalCreditsAttribute>(Assembly);
                        var procAttrs = LoaderUtils.PullAttributesFromAssembly<MelonProcessAttribute>(Assembly);
                        var gameAttrs = LoaderUtils.PullAttributesFromAssembly<MelonGameAttribute>(Assembly);
                        var optionalDependenciesAttr = LoaderUtils.PullAttributeFromAssembly<MelonOptionalDependenciesAttribute>(Assembly);
                        var idAttr = LoaderUtils.PullAttributeFromAssembly<MelonIDAttribute>(Assembly);
                        var gameVersionAttrs = LoaderUtils.PullAttributesFromAssembly<MelonGameVersionAttribute>(Assembly);
                        var platformAttr = LoaderUtils.PullAttributeFromAssembly<MelonPlatformAttribute>(Assembly);
                        var domainAttr = LoaderUtils.PullAttributeFromAssembly<MelonPlatformDomainAttribute>(Assembly);
                        var mlVersionAttr = LoaderUtils.PullAttributeFromAssembly<VerifyLoaderVersionAttribute>(Assembly);
                        var mlBuildAttr = LoaderUtils.PullAttributeFromAssembly<VerifyLoaderBuildAttribute>(Assembly);

                        mod.Info = info;
                        mod.AdditionalCredits = additionalCreditsAttr;
                        mod.ModAssembly = this;
                        mod.Priority = priorityAttr?.Priority ?? 0;
                        mod.ConsoleColor = colorAttr?.DrawingColor ?? RLog.DefaultMelonColor;
                        mod.AuthorConsoleColor = authorColorAttr?.DrawingColor ?? RLog.DefaultTextColor;
                        //melon.SupportedProcesses = procAttrs;
                        //melon.Games = gameAttrs;
                        //melon.SupportedGameVersion = gameVersionAttrs?.;
                        //melon.SupportedPlatforms = platformAttr;
                        //melon.SupportedDomain = domainAttr;
                        //melon.SupportedMLVersion = mlVersionAttr;
                        //melon.SupportedMLBuild = mlBuildAttr;
                        //melon.OptionalDependencies = optionalDependenciesAttr.AssemblyNames;
                        //melon.OptionalDependencies = Array.Empty<string>();
                        mod.ID = idAttr?.ID;

                        loadedMelons.Add(mod);

                        if (!SemVersion.TryParse(info.Version, out _))
                            RLog.Warning($"==Normal users can ignore this warning==\nMelon '{info.Name}' by '{info.Author}' has version '{info.Version}' which does not use the Semantic Versioning format. Versions using formats other than the Semantic Versioning format will not be supported in the future versions of RedLoader.\nFor more details, see: https://semver.org");
                    }
                    else
                    {
                        RLog.Warning($"Skipping ({info.Name}) since it is written for the original RedLoader.");
                    }
                }
            }

            RegisterTypeInIl2Cpp.RegisterAssembly(Assembly);

            if (rottenMelons.Count != 0)
            {
                RLog.Error($"Failed to load {rottenMelons.Count} {"Melon".MakePlural(rottenMelons.Count)} from {Path.GetFileName(Location)}:");
                foreach (var r in rottenMelons)
                {
                    RLog.Error($"Failed to load Melon '{r.type.FullName}': {r.errorMessage}");
                    if (r.exception != null)
                        RLog.Error(r.exception);
                }
            }
        }

        #endregion
    }
}