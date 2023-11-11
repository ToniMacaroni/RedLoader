using System.Drawing;
using System.Reflection;
using RedLoader;
using RedLoader.Utils;
using SonsSdk;
using SonsSdk.Attributes;

[assembly: MelonInfo(typeof(SonsLoaderPlugin.Core), "SonsLoaderPlugin", "1.0.0", "Toni Macaroni")]
[assembly: MelonColor(0, 255, 20, 255)]

namespace SonsLoaderPlugin;

public class Core : LoaderPlugin
{
    public override void OnPreModsLoaded()
    {
        MelonAssembly.CustomMelonResolvers += Resolver;
    }

    protected override void OnLateInitializeMod()
    {
        SdkEvents.Init();
    }

    private ResolvedMelons Resolver(MelonAssembly melonAssembly)
    {
        var assembly = melonAssembly.Assembly;
        
        var melons = new List<ModBase>();
        var rottenMelons = new List<RottenMelon>();

        var path = LoaderEnvironment.GetMetadataPath(melonAssembly.Assembly);
        var manifest = ManifestReader.TryReadManifest(path);
        if (manifest != null)
        {
            if (manifest.Platform == "Client" && LoaderEnvironment.IsDedicatedServer)
            {
                RLog.Error($"Mod {assembly.FullName} is a client mod and cannot be loaded on a dedicated server.");
                ModReport.ReportMod(manifest.Id, "Client mod cannot be loaded on a dedicated server.");
            }
            else if (manifest.Platform == "Server" && !LoaderEnvironment.IsDedicatedServer)
            {
                RLog.Error($"Mod {assembly.FullName} is a server mod and cannot be loaded on a client.");
                ModReport.ReportMod(manifest.Id, "Server mod cannot be loaded on a client.");
            }
            else if (!string.IsNullOrEmpty(manifest.LoaderVersion) && !LoaderUtils.IsCompatible(manifest.LoaderVersion))
            {
                RLog.Error($"Mod {assembly.FullName} requires a different version of RedLoader.");
                //LoaderUtils.ShowMessageBox($"Mod {manifest.Id} requires a newer version of RedLoader.");
                ModReport.ReportMod(manifest.Id, $"Requires RedLoader >={manifest.LoaderVersion}");
            }
            else if (InitMod(melonAssembly, manifest, out var mod))
            {
                RLog.Msg(System.ConsoleColor.Magenta, $"Loaded mod {mod.Info.Name}");
                
                melons.Add(mod);

                mod.AssetBundleAttrs = AssetBundleAttributeLoader.GetAllTypes(mod);
            }
        }
        else
        {
            RLog.Error($"{assembly.FullName} does not have a manifest.json file.");
            ModReport.ReportMod(assembly.GetName().Name, "Missing manifest.json");
        }
        
        return new ResolvedMelons(melons.ToArray(), rottenMelons.ToArray());
    }

    private bool InitMod(MelonAssembly melonAssembly, ManifestData data, out SonsMod outMod)
    {
        outMod = null;
        
        var assembly = melonAssembly.Assembly;
        var implementation = assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(SonsMod)));
        if (implementation == null)
        {
            RLog.Error($"Failed to find a valid implementation of SonsMod in {assembly.FullName}");
            return false;
        }
        
        try
        {
            outMod = (SonsMod)Activator.CreateInstance(implementation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
            
            if(outMod == null)
            {
                RLog.Error($"Failed to create an instance of {implementation.FullName}");
                return false;
            }
            
            var version = new Version(data.Version);
            data.VersionObject = version;
                
            outMod.Info = new MelonInfoAttribute(
                outMod.GetType(), 
                string.IsNullOrEmpty(data.Name) ? data.Id : data.Name, 
                version.Major, 
                version.Minor,
                version.Build,
                data.Author,
                data.Url);

            outMod.Manifest = data;
                
            outMod.ModAssembly = melonAssembly;
            outMod.Priority = data.Priority;
            outMod.ConsoleColor = LoaderUtils.ColorFromString(data.LogColor);
            outMod.AuthorConsoleColor = RLog.DefaultTextColor;
            outMod.SupportedGameVersion = data.GameVersion;
            outMod.OptionalDependencies = data.Dependencies;
            outMod.ID = data.Id;
            
            return true;
        }
        catch (Exception e)
        {
            RLog.Error(e);
            return false;
        }
    }
}