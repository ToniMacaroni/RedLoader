using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using DefaultNamespace;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Octokit;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using FileMode = System.IO.FileMode;
using Project = Nuke.Common.ProjectModel.Project;

class Build : NukeBuild
{
    public static int Main ()
    {
        return Execute<Build>(x => x.Compile);
    }
    
    [GitVersion] GitVersion GitVersion;
    [GitRepository] GitRepository GitRepository;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    static Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Game path")] static AbsolutePath GamePath;
    
    [Parameter("Should the build be copied to the game folder")] static bool ShouldCopyToGame = false;

    [Parameter("Run the game after completion")] static bool StartGame;
    
    [Parameter("Restore packages")] static bool RestorePackages = false;
    
    [Parameter("Github token")] static string GithubToken = "";
    
    [Parameter("Load into main")] static bool LoadIntoMain = false;
    
    [Parameter("Load Savegame")] static string LoadSave = "";
    
    [Parameter("Test Pack in game")] static bool TestPack = false;
    
    [Parameter("Debug")] static bool Debug = false;
    
    [Parameter("No Zip")] static bool NoZip = false;
    
    const string ProjectAlias = "RedLoader";
    static string ProjectFolder => "_" + ProjectAlias;
    static AbsolutePath OutputDir => RootDirectory / "Output" / Configuration / ProjectFolder;

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    [PathVariable] static Tool Cargo;

    static bool IsWindows64;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            foreach (var project in Solution.AllProjects)
            {
                try
                {
                    (project.Directory / "bin").CreateOrCleanDirectory();
                }
                catch (Exception e)
                {
                    Serilog.Log.Warning("Failed to clean project {Project}", project.Name);
                }
            }

            (RootDirectory / "Output").CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .OnlyWhenStatic(() => RestorePackages)
        .WhenSkipped(DependencyBehavior.Execute)
        .Executes(() =>
        {
            DotNetRestore();
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var generatedAssembliesExist = (GamePath / ProjectFolder / "Game").DirectoryExists();

            IsWindows64 = true;
            //DotNetBuild(x => x.SetNoConsoleLogger(true));
            foreach (var project in Solution.AllProjects)
            {
                if(project.Name == "_build")
                    continue;
                
                if(project.Name.Contains("Sons") && !generatedAssembliesExist)
                    continue;
                
                BuildToOutput(project, ShouldCopyToGame);
            }

            if (!generatedAssembliesExist)
            {
                Serilog.Log.Warning("===> No generated assemblies found, generating them now on game start");
            }
            
            if(!(GamePath / "version.dll").FileExists())
            {
                Serilog.Log.Information("===> Didn't find native dependencies in the game folder. Copying/Building them now.");
                CopyBuiltDependencies(GamePath);
            }

            if(StartGame)
                RunGame();
        });
    
    Target CompileSdk => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            //DotNetBuild(x => x.SetNoConsoleLogger(true));
            //BuildToOutput(Solution.SonsSdk, true);
            //BuildToOutput(Solution.SonsGameManager, true);
            //BuildToOutput(Solution.SonsLoaderPlugin, true);
            
            DotNetBuild(x => x
                .SetProjectFile(Solution.FileName)
                .EnableNoRestore()
                .EnableNoLogo()
                .SetConfiguration(Configuration)
                .SetPlatform("Windows - x64")
                .SetAssemblyVersion(GitVersion.MajorMinorPatch)
                .SetFileVersion(GitVersion.MajorMinorPatch)
                //.SetOutputDirectory(skipOutput ? x.OutputDirectory : newOutputPath)
                .SetNoConsoleLogger(true));
            
            Serilog.Log.Information($"===> Copying to game folder");
            CopyToGame(Solution.SonsSdk);
            CopyToGame(Solution.SonsLoaderPlugin);
            CopyToGame(Solution.SonsGameManager);
            
            if(StartGame)
                RunGame();
        });
    
    Target Pack => _ => _
        .DependsOn(Restore)
        .Requires(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            Serilog.Log.Information("=============================");
            Serilog.Log.Information($"===   Packing for {GitVersion.MajorMinorPatch}   ===");
            Serilog.Log.Information("=============================");

            if (!string.IsNullOrEmpty(GamePath))
            {
                Serilog.Log.Information("Game Folder: {GamePath}", GamePath);
            }
            
            var generatedAssembliesExist = !string.IsNullOrEmpty(GamePath) && (GamePath / ProjectFolder / "Game").DirectoryExists();

            IsWindows64 = true;

            foreach (var project in Solution.AllProjects)
            {
                if(project.Name == "_build")
                    continue;
                
                if(project.Name.Contains("Sons") && !generatedAssembliesExist)
                    continue;

                if (project == Solution.ImGuiWindow)
                {
                    var dll = (project.Directory / "bin" / Configuration / project.Name + ".dll");
                    if(dll.FileExists())
                        CopyFileToDirectory(dll, OutputDir / "net6", FileExistsPolicy.Overwrite);
                    continue;
                }
                
                BuildToOutput(project);
            }
            
            var bgPath = RootDirectory / "Resources" / "bg.png";
            if(bgPath.FileExists())
                CopyFileToDirectory(bgPath, OutputDir, FileExistsPolicy.Overwrite);
            
            Serilog.Log.Information("===> Copying manifests");
            
            // copy manifests
            foreach (var project in Solution.AllProjects)
            {
                if (project == Solution.ImGuiWindow)
                    continue;
                
                var outputPath = GetBuildOutputPath(project);
                var (assemblyName, _) = GetBuiltAssemblyPath(project);
                var manifest = outputPath / "manifest.json";
                if(!manifest.FileExists())
                    continue;
                CopyFileToDirectory(manifest, OutputDir / GetRelativeOutputPath(project) / assemblyName.Name.Replace(".dll", ""), FileExistsPolicy.Overwrite);
            }
            
            if (!generatedAssembliesExist)
            {
                Serilog.Log.Warning("===> No generated assemblies found, generating them now on game start");
            }

            CopyBuiltDependencies(OutputDir / "..");

            var zip = RootDirectory / $"{ProjectAlias}.zip";
            
            if(zip.FileExists())
                zip.DeleteFile();
            
            if(!NoZip)
                (OutputDir / "..").ZipTo(zip, compressionLevel: CompressionLevel.SmallestSize, fileMode:FileMode.CreateNew);

            if (GamePath.DirectoryExists() && TestPack)
            {
                Serilog.Log.Information("===> Copying to game folder for testing");
                Serilog.Log.Information("=> Deleting old files");
                
                var loaderDir = GamePath / ProjectFolder;
                if (loaderDir.DirectoryExists())
                    loaderDir.DeleteDirectory();
                
                var dobby = GamePath / "dobby.dll";
                if (dobby.FileExists())
                    dobby.DeleteFile();
                
                var version = GamePath / "version.dll";
                if (version.FileExists())
                    version.DeleteFile();

                Serilog.Log.Information("=> Files deleted");
                Serilog.Log.Information("=> Press enter to continue...");
                Console.ReadLine();
                
                Serilog.Log.Information("=> Copying new files");
                
                zip.UnZipTo(GamePath);
                
                Serilog.Log.Information("=> Files copied");
                Serilog.Log.Information("=> Press enter to start the game...");
                Console.ReadLine();
            }
            
            else if (GamePath.DirectoryExists() && ShouldCopyToGame)
            {
                CopyToGame(Solution.SonsSdk);
                CopyToGame(Solution.RedLoader);
                CopyToGame(Solution.SonsLoaderPlugin);
                CopyToGame(Solution.SonsGameManager);
                
                var imguiwindowDLL = (Solution.ImGuiWindow.Directory / "bin" / Configuration / "ImGuiWindow.dll");
                Serilog.Log.Information($"imgui window in {imguiwindowDLL}");
                if (imguiwindowDLL.FileExists())
                {
                    Serilog.Log.Information("Copied imgui window");
                    CopyFileToDirectory(imguiwindowDLL, GamePath / ProjectFolder / "net6", FileExistsPolicy.Overwrite);
                }
            }
            
            if((TestPack || ShouldCopyToGame) && StartGame)
                RunGame();
        });

    Target ProcessDoc => _ => _
        .Executes( async () =>
        {
            var docProcessor = new DocProcessor(new List<AbsolutePath>
            {
                RootDirectory / "Output" / "Release" / ProjectFolder / "net6" / ProjectAlias + ".xml",
                RootDirectory / "Output" / "Release" / ProjectFolder / "net6" / "SonsSdk.xml",
                RootDirectory / "SonsGameManager" / "SonsGameManager.xml",
            });
            
            docProcessor.ProcessReadme(RootDirectory / "Resources" / "README_TEMPLATE.md", RootDirectory / "README.md");

            await Docfx.Docset.Build(Path.Combine("docfx_project", "docfx.json"));
        });

    static void BuildRustDependencies() => Cargo(arguments: "+nightly build --target x86_64-pc-windows-msvc --release", workingDirectory: RootDirectory);

    /// <summary>
    /// Copy rust built binaries (or build them if they don't exist) to the specified directory
    /// </summary>
    /// <param name="dir"></param>
    static void CopyBuiltDependencies(AbsolutePath dir)
    {
        if(!(RootDirectory / "target" / "x86_64-pc-windows-msvc" / "release" / "Bootstrap.dll").FileExists())
            BuildRustDependencies();
        
        CopyFileToDirectory(RootDirectory / "target" / "x86_64-pc-windows-msvc" / "release" / "Bootstrap.dll", dir / ProjectFolder / "Dependencies",
            FileExistsPolicy.Overwrite);
        CopyFileToDirectory(RootDirectory / "target" / "x86_64-pc-windows-msvc" / "release" / "version.dll", dir,
            FileExistsPolicy.Overwrite);
        CopyFileToDirectory(RootDirectory / "BaseLibs" / "dobby_x64.dll", dir, FileExistsPolicy.Overwrite);
        RenameFile(dir / "dobby_x64.dll", "dobby.dll", FileExistsPolicy.Overwrite);
    }

    static void RunGame()
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = GamePath / "SonsOfTheForest.exe",
            WorkingDirectory = GamePath,
            UseShellExecute = false,
            //Arguments = "--melonloader.launchdebugger"
        };

        if (LoadIntoMain)
            processInfo.Arguments = "--sdk.loadintomain";
        
        if(!string.IsNullOrEmpty(LoadSave))
            processInfo.Arguments = "--savegame " + LoadSave;

        if (Debug) 
            processInfo.Arguments += "--launchdebugger --debug";

        Process.Start(processInfo);
    }

    Target Upload => _ => _
        .Requires(() => GithubToken != "")
        .Executes(() =>
        {
            GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(nameof(NukeBuild)))
            {
                Credentials = new Credentials(GithubToken)
            };
            
            var changeLog = RootDirectory / "CHANGELOG.md";
            var changeLogContent = changeLog.ReadAllText();

            var release = new NewRelease(GitVersion.MajorMinorPatch)
            {
                Name = $"{ProjectAlias} {GitVersion.MajorMinorPatch}",
                Body = changeLogContent
            };

            var createdRelease = GitHubTasks.GitHubClient.Repository.Release
                .Create(GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName(), release).Result;
            
            UploadReleaseAssetToGithub(createdRelease, RootDirectory / $"{ProjectAlias}.zip");
        });

    IReadOnlyCollection<Output> BuildToOutput(Project project, bool copyToGame = false)
    {
        var newOutputPath = GetTargetOutputPath(project);

        var copyMode = GetBoolProp(project, "CopyToOuput");
        var dontOutput = GetBoolProp(project, "DontOuput");

        Serilog.Log.Information($"===> Building {project.Name} to {newOutputPath}");
        
        var build = DotNetBuild(x => SetAdditionalSettings(project, x)
                .SetProjectFile(project)
                .EnableNoRestore()
                .EnableNoLogo()
                .SetConfiguration(Configuration)
                .SetPlatform("Windows - x64")
                .SetAssemblyVersion(GitVersion.MajorMinorPatch)
                .SetFileVersion(GitVersion.MajorMinorPatch)
                //.SetOutputDirectory(skipOutput ? x.OutputDirectory : newOutputPath)
                .SetNoConsoleLogger(true));
        
        if(copyToGame)
        {
            CopyToGame(project);
        }
        else
        {
            if (copyMode)
            {
                CopyBuiltFiles(project, newOutputPath);
            } 
            else if (!dontOutput)
            {
                CopyDirectoryRecursively(
                    project.Directory / "bin" / "Windows - x64" / Configuration, 
                    OutputDir / GetRelativeOutputPath(project), 
                    DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
            }
        }

        return build;
    }

    void CopyToGame(Project project)
    {
        if(string.IsNullOrEmpty(GamePath))
        {
            Serilog.Log.Error("GamePath is not set");
            return;
        }

        AbsolutePath gamePath = GamePath / ProjectFolder;

        if(HasTargetFrameworkAppended(project))
            gamePath /= project.GetProperty("TargetFramework");
        
        gamePath /= GetRelativeOutputPath(project);
        
        CopyBuiltFiles(project, gamePath);
    }

    void CopyBuiltFiles(Project project, AbsolutePath to)
    {
        var (dll, pdb) = GetBuiltAssemblyPath(project);
        CopyFileToDirectory(dll, to, FileExistsPolicy.Overwrite);
        if(pdb.FileExists())
            CopyFileToDirectory(pdb, to, FileExistsPolicy.Overwrite);
    }
    
    AbsolutePath GetTargetOutputPath(Project project)
    {
        var ouput = OutputDir / GetRelativeOutputPath(project);
        
        if(HasTargetFrameworkAppended(project))
            ouput /= project.GetProperty("TargetFramework");
        
        return ouput;
    }
    
    AbsolutePath GetBuildOutputPath(Project project)
    {
        var output = project.Directory / "bin" / "Windows - x64" / Configuration;
        if (!IsWindows64) 
            output = project.Directory / "bin" / Configuration;
        
        if(HasTargetFrameworkAppended(project))
            output /= project.GetProperty("TargetFramework");
        
        return output;
    }
    
    (AbsolutePath dll, AbsolutePath pdb) GetBuiltAssemblyPath(Project project)
    {
        var ouput = GetBuildOutputPath(project);
        var assemblyName = project.GetProperty("AssemblyName");
        
        if(string.IsNullOrEmpty(assemblyName))
            assemblyName = project.Name;
        
        var dllPath = ouput / $"{assemblyName}.dll";
        var pdbPath = ouput / $"{assemblyName}.pdb";
        return (dllPath, pdbPath);
    }

    bool GetBoolProp(Project project, string name, bool defaultValue = false)
    {
        var prop = project.GetProperty(name);
        if (string.IsNullOrEmpty(prop))
            return defaultValue;
        return bool.Parse(prop);
    }

    bool HasTargetFrameworkAppended(Project project)
    {
        return GetBoolProp(project, "AppendTargetFrameworkToOutputPath", true);
    }

    string GetRelativeOutputPath(Project project)
    {
        return project.GetProperty("RelativeOutputPath");
    }
    
    void UploadReleaseAssetToGithub(Release release, AbsolutePath asset)
    {
        if (!asset.FileExists())
        {
            return;
        }

        var assetContentType = "application/x-binary";

        var releaseAssetUpload = new ReleaseAssetUpload
        {
            ContentType = assetContentType,
            FileName = Path.GetFileName(asset),
            RawData = File.OpenRead(asset)
        };
        var _ = GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, releaseAssetUpload).Result;
    }

    DotNetBuildSettings SetAdditionalSettings(Project project, DotNetBuildSettings settings)
    {
        if (project.Name != "RedLoader")
        {
            return settings;
        }

        settings = settings.SetTitle("RedLoader based on RedLoader");
        settings = settings.SetDescription("RedLoader based on RedLoader");
        settings = settings.SetAuthors("Lava Gang & Toni Macaroni");
        settings = settings.SetCopyright("Created by Lava Gang & Toni Macaroni");
        return settings;
    }
}