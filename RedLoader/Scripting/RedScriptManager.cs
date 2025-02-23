using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mono.Cecil;
using RedLoader.Utils;
using UnityEngine;
using Color = System.Drawing.Color;

namespace RedLoader.Scripting;

internal class RedScriptManager
{
    private const float AUTO_RELOAD_DELAY = 1.0f;

    private static FileSystemWatcher _fileSystemWatcher;
    private static bool _shouldReload;
    private static float _autoReloadTimer;
    private static Dictionary<string, RedScript> _loadedScripts = new();
    private static HashSet<string> _scriptQueue = new();

    public static void Init()
    {
        Directory.CreateDirectory(LoaderEnvironment.ScriptDirectory);
        
        ReloadPlugins();
        StartFileSystemWatcher();
    }

    public static void Update()
    {
        if (_shouldReload)
        {
            _autoReloadTimer -= Time.unscaledDeltaTime;
            if (_autoReloadTimer <= .0f)
            {
                ProcessScriptQueue();
            }
        }
    }

    private static void ReloadPlugins()
    {
        _shouldReload = false;
        
        RLog.Msg(Color.DarkGray, "Unloading scripts");
        
        foreach (var loadedScript in _loadedScripts.Values)
        {
            loadedScript.OnUnload();
        }

        RLog.Msg(Color.DarkGray, "Unloaded scripts");
        
        _loadedScripts.Clear();

        var files = Directory.GetFiles(LoaderEnvironment.ScriptDirectory, "*.dll");
        foreach (string path in files)
            LoadDLL(path);

        if(files.Length > 0)
            RLog.Msg(Color.DarkGray, "Reloaded all plugins!");
    }

    private static void LoadScript(string path)
    {
        if (_loadedScripts.TryGetValue(path, out var script))
        {
            RLog.Msg(Color.DarkGray, "Unloading script");
            script.OnUnload();
            _loadedScripts.Remove(path);
            RLog.Msg(Color.DarkGray, "Unloaded script");
        }
        
        LoadDLL(path);
        RLog.Msg(Color.DarkGray, "Reloaded plugin!");
    }

    private static void ProcessScriptQueue()
    {
        _shouldReload = false;

        foreach (var script in _scriptQueue)
        {
            LoadScript(script);
        }
        
        _scriptQueue.Clear();
    }

    private static void LoadDLL(string path)
    {
        var defaultResolver = new DefaultAssemblyResolver();
        defaultResolver.AddSearchDirectory(LoaderEnvironment.ScriptDirectory);
        defaultResolver.AddSearchDirectory(LoaderEnvironment.LoaderAssemblyDirectory);
        defaultResolver.AddSearchDirectory(LoaderEnvironment.Il2CppAssembliesDirectory);

        RLog.Msg(Color.DarkGray, $"Loading plugins from {path}");

        using (var dll = AssemblyDefinition.ReadAssembly(path, new ReaderParameters {
                   AssemblyResolver = defaultResolver,
                   ReadSymbols = true
               }))
        {
            dll.Name.Name = $"{dll.Name.Name}-{DateTime.Now.Ticks}";
            Assembly ass;
            
            using (var ms = new MemoryStream())
            {
                dll.Write(ms);
                ass = Assembly.Load(ms.ToArray());
            }
            
            foreach (Type type in GetTypesSafe(ass))
            {
                try
                {
                    if (!typeof(RedScript).IsAssignableFrom(type)) continue;
            
                    RLog.Msg(Color.DarkGray, $"Loading {type.Name}");
            
                    DelayAction(() =>
                    {
                        try
                        {
                            var script = (RedScript)Activator.CreateInstance(type);
                            if (script != null)
                            {
                                _loadedScripts[path] = script;
                                script?.OnLoad();
                                RLog.Msg(Color.GreenYellow, "Loading Successful");
                            }
                        }
                        catch (Exception e)
                        {
                            RLog.Error($"Failed to load {type.Name} because of exception: {e}");
                        }
                    }).RunCoro();
                }
                catch (Exception e)
                {
                    RLog.Error($"Failed to load plugin {type.Name} because of exception: {e}");
                }
            }
        }
    }

    private static void StartFileSystemWatcher()
    {
        _fileSystemWatcher = new FileSystemWatcher(LoaderEnvironment.ScriptDirectory)
        {
            IncludeSubdirectories = false
        };
        _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
        _fileSystemWatcher.Filter = "*.dll";
        _fileSystemWatcher.Changed += FileChangedEventHandler;
        _fileSystemWatcher.Deleted += FileChangedEventHandler;
        _fileSystemWatcher.Created += FileChangedEventHandler;
        _fileSystemWatcher.Renamed += FileChangedEventHandler;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    private static void FileChangedEventHandler(object sender, FileSystemEventArgs args)
    {
        if (_scriptQueue.Contains(args.FullPath))
            return;
        
        RLog.Msg(Color.DarkGray, $"File {args.Name} changed. Processing shortly...");
        _scriptQueue.Add(args.FullPath);
        _shouldReload = true;
        _autoReloadTimer = AUTO_RELOAD_DELAY;
    }

    private static IEnumerable<Type> GetTypesSafe(Assembly ass)
    {
        try
        {
            return ass.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var sbMessage = new StringBuilder();
            sbMessage.AppendLine("\r\n-- LoaderExceptions --");
            foreach (var l in ex.LoaderExceptions)
                sbMessage.AppendLine(l.ToString());
            sbMessage.AppendLine("\r\n-- StackTrace --");
            sbMessage.AppendLine(ex.StackTrace);
            RLog.Error(sbMessage.ToString());
            return ex.Types.Where(x => x != null);
        }
    }

    private static IEnumerator DelayAction(Action action)
    {
        yield return null;
        action();
    }
}
