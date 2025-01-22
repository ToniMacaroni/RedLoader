#pragma warning disable CS1591
// ReSharper disable ClassNeverInstantiated.Global
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.NuGet.Push;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Git;
using Cake.Json;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;

return new CakeHost()
       .UseContext<BuildContext>()
       .Run(args);

public class BuildContext : FrostingContext
{
    public enum ProjectBuildType
    {
        Release,
        Development
    }

    public const string DoorstopVersion = "4.3.0";
    public const string DotnetRuntimeVersion = "6.0.7";
    public const string DobbyVersion = "1.0.5";

    public const string DotnetRuntimeZipUrl =
        $"https://github.com/BepInEx/dotnet-runtime/releases/download/{DotnetRuntimeVersion}/mini-coreclr-Release.zip";

    internal readonly DistributionTarget[] Distributions =
    {
        new("Unity.IL2CPP", "win-x64"),
        // new("Unity.IL2CPP", "linux-x64"),
    };


    public BuildContext(ICakeContext ctx)
        : base(ctx)
    {
        RootDirectory = ctx.Environment.WorkingDirectory.GetParent();
        OutputDirectory = RootDirectory.Combine("bin");
        CacheDirectory = OutputDirectory.Combine(".dep_cache");
        DistributionDirectory = OutputDirectory.Combine("dist");
        var props = Project.FromFile(RootDirectory.CombineWithFilePath("Directory.Build.props").FullPath,
                                     new ProjectOptions());
        VersionPrefix = props.GetPropertyValue("VersionPrefix");
        CurrentCommit = ctx.GitLogTip(RootDirectory);

        BuildType = ctx.Argument("build-type", ProjectBuildType.Release);
        // BuildType = ProjectBuildType.Development;
        BuildId = ctx.Argument("build-id", -1);
        LastBuildCommit = ctx.Argument("last-build-commit", "");
        NugetApiKey = ctx.Argument("nuget-api-key", "");
        NugetSource = ctx.Argument("nuget-source", "https://nuget.bepinex.dev/v3/index.json");
    }

    public ProjectBuildType BuildType { get; }
    public int BuildId { get; }
    public string LastBuildCommit { get; }
    public string NugetApiKey { get; }
    public string NugetSource { get; }

    public DirectoryPath RootDirectory { get; }
    public DirectoryPath OutputDirectory { get; }
    public DirectoryPath CacheDirectory { get; }
    public DirectoryPath DistributionDirectory { get; }

    public string VersionPrefix { get; }
    public GitCommit CurrentCommit { get; }

    public string VersionSuffix => BuildType switch
    {
        ProjectBuildType.Release      => "",
        ProjectBuildType.Development  => "dev",
        var _                         => throw new ArgumentOutOfRangeException()
    };

    public static string DoorstopZipUrl(string arch) =>
        $"https://github.com/NeighTools/UnityDoorstop/releases/download/v{DoorstopVersion}/doorstop_{arch}_release_{DoorstopVersion}.zip";

    public static string DobbyZipUrl(string arch) =>
        $"https://github.com/BepInEx/Dobby/releases/download/v{DobbyVersion}/dobby-{arch}.zip";
}

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext ctx)
    {
        ctx.CreateDirectory(ctx.OutputDirectory);
        ctx.CleanDirectory(ctx.OutputDirectory,
                           f => !f.Path.FullPath.Contains(".dep_cache"));

        ctx.Log.Information("Cleaning up old build objects");
        ctx.CleanDirectories(ctx.RootDirectory.Combine("**/RedLoader.*/**/bin").FullPath);
        ctx.CleanDirectories(ctx.RootDirectory.Combine("**/RedLoader.*/**/obj").FullPath);
    }
}

[TaskName("Compile")]
[IsDependentOn(typeof(CleanTask))]
public sealed class CompileTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext ctx)
    {
        var buildSettings = new DotNetBuildSettings
        {
            Configuration = ctx.BuildType == BuildContext.ProjectBuildType.Release ? "Release" : "Debug"
        };
        if (ctx.BuildType != BuildContext.ProjectBuildType.Release)
        {
            buildSettings.MSBuildSettings = new()
            {
                VersionSuffix = ctx.VersionSuffix,
                Properties =
                {
                    ["SourceRevisionId"] = new[] { ctx.CurrentCommit.Sha },
                    ["RepositoryBranch"] = new[] { ctx.GitBranchCurrent(ctx.RootDirectory).FriendlyName }
                }
            };
        }

        ctx.DotNetBuild(ctx.RootDirectory.FullPath, buildSettings);
    }
}

[TaskName("DownloadDependencies")]
public sealed class DownloadDependenciesTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext ctx)
    {
        ctx.Log.Information("Downloading dependencies");
        ctx.CreateDirectory(ctx.CacheDirectory);

        var cache = new DependencyCache(ctx, ctx.CacheDirectory.CombineWithFilePath("cache.json"));

        cache.Refresh("NeighTools/UnityDoorstop", BuildContext.DoorstopVersion, () =>
        {
            ctx.Log.Information($"Downloading Doorstop {BuildContext.DoorstopVersion}");
            var doorstopDir = ctx.CacheDirectory.Combine("doorstop");
            ctx.CreateDirectory(doorstopDir);
            ctx.CleanDirectory(doorstopDir);
            var archs = new[] { "win", "linux", "macos" };
            var versions = archs
                           .Select(a => ($"Doorstop ({a})",
                                         BuildContext.DoorstopZipUrl(a),
                                         doorstopDir.Combine($"doorstop_{a}")))
                           .ToArray();
            ctx.DownloadZipFiles($"Doorstop {BuildContext.DoorstopVersion}", versions);
        });

        cache.Refresh("BepInEx/Dobby", BuildContext.DobbyVersion, () =>
        {
            ctx.Log.Information($"Downloading Dobby {BuildContext.DobbyVersion}");
            var dobbyDir = ctx.CacheDirectory.Combine("dobby");
            ctx.CreateDirectory(dobbyDir);
            ctx.CleanDirectory(dobbyDir);
            var archs = new[] { "win", "linux", "macos" };
            var versions = archs
                           .Select(a => ($"Dobby ({a})", BuildContext.DobbyZipUrl(a), dobbyDir.Combine($"dobby_{a}")))
                           .ToArray();
            ctx.DownloadZipFiles($"Dobby {BuildContext.DobbyVersion}", versions);
        });

        cache.Refresh("BepInEx/dotnet_runtime", BuildContext.DotnetRuntimeVersion, () =>
        {
            ctx.Log.Information($"Downloading dotnet runtime {BuildContext.DotnetRuntimeVersion}");
            var dotnetDir = ctx.CacheDirectory.Combine("dotnet");
            ctx.CreateDirectory(dotnetDir);
            ctx.CleanDirectory(dotnetDir);
            ctx.DownloadZipFiles($"dotnet-runtime {BuildContext.DotnetRuntimeVersion}",
                                 ("dotnet runtime", BuildContext.DotnetRuntimeZipUrl, dotnetDir));
        });

        cache.Save();
    }
}

[TaskName("MakeDist")]
[IsDependentOn(typeof(CompileTask))]
[IsDependentOn(typeof(DownloadDependenciesTask))]
public sealed class MakeDistTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext ctx)
    {
        ctx.CreateDirectory(ctx.DistributionDirectory);
        ctx.CleanDirectory(ctx.DistributionDirectory);

        foreach (var dist in ctx.Distributions)
        {
            ctx.Log.Information($"Creating distribution {dist.Target}");
            var targetDir = ctx.DistributionDirectory.Combine(dist.Target);
            ctx.CreateDirectory(targetDir);
            ctx.CleanDirectory(targetDir);

            var redloaderDir = targetDir.Combine("_Redloader");
            var net6Dir = redloaderDir.Combine("net6");
            ctx.CreateDirectory(redloaderDir);
            ctx.CreateDirectory(net6Dir);
            ctx.CreateDirectory(targetDir.Combine("Mods"));
            ctx.CreateDirectory(redloaderDir.Combine("Patchers"));

            //File.WriteAllText(targetDir.CombineWithFilePath("changelog.txt").FullPath, changelog);

            var sourceDirectory = ctx.OutputDirectory.Combine(dist.DistributionIdentifier);
            if (dist.FrameworkTarget != null)
                sourceDirectory = sourceDirectory.Combine(dist.FrameworkTarget);

            foreach (var filePath in ctx.GetFiles(sourceDirectory.Combine("*.*").FullPath))
                ctx.CopyFileToDirectory(filePath, net6Dir);

            var doorstopPath =
                ctx.CacheDirectory.Combine("doorstop").Combine($"doorstop_{dist.Os}").Combine(dist.Arch);
            ctx.CopyFile(doorstopPath.GetFilePath("winhttp.dll"), targetDir.GetFilePath("version.dll"));
            // foreach (var filePath in ctx.GetFiles(doorstopPath.Combine($"*.{dist.DllExtension}").FullPath))
            //     ctx.CopyFileToDirectory(filePath, targetDir);
            // ctx.CopyFileToDirectory(doorstopPath.CombineWithFilePath(".doorstop_version"), targetDir);
            var (doorstopConfigFile, doorstopConfigDistName) = dist.Os switch
            {
                "win" => ($"doorstop_config_{dist.Runtime.ToLower()}.ini",
                          "doorstop_config.ini"),
                "linux" or "macos" => ($"run_bepinex_{dist.Runtime.ToLower()}.sh",
                                       "run_bepinex.sh"),
                var _ => throw new
                             NotSupportedException(
                                                   $"Doorstop is not supported on {dist.Os}")
            };
            ctx.CopyFile(ctx.RootDirectory.Combine("Doorstop").CombineWithFilePath(doorstopConfigFile),
                         targetDir.CombineWithFilePath(doorstopConfigDistName));

            ctx.CopyFile(ctx.CacheDirectory.Combine("dobby").Combine($"dobby_{dist.Os}").CombineWithFilePath($"{dist.DllPrefix}dobby_{dist.Arch}.{dist.DllExtension}"),
                         net6Dir.CombineWithFilePath($"{dist.DllPrefix}dobby.{dist.DllExtension}"));
            ctx.CopyDirectory(ctx.CacheDirectory.Combine("dotnet").Combine(dist.RuntimeIdentifier),
                              redloaderDir.Combine("dotnet"));
            ctx.CopyFile(ctx.RootDirectory.Combine("Libs").GetFilePath("Splash.dll"), net6Dir.GetFilePath("Splash.dll"));
            ctx.CopyFile(ctx.RootDirectory.Combine("Resources").GetFilePath("bg.png"), redloaderDir.GetFilePath("bg.png"));
        }
    }
}

[TaskName("MakeZip")]
[IsDependentOn(typeof(MakeDistTask))]
public sealed class MakeZipTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext ctx)
    {
        foreach (var dist in ctx.Distributions)
        {
            var dir = ctx.DistributionDirectory.Combine(dist.Target);
            ctx.Zip(dir, ctx.DistributionDirectory.GetFilePath("Redloader.zip"));
        }
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(CompileTask))]
public class DefaultTask : FrostingTask { }
