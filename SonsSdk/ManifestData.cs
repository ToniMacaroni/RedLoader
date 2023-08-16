using MelonLoader.TinyJSON;

namespace SonsSdk;

public class ManifestData
{
    /// <summary>
    /// Unique id of the mod
    /// </summary>
    [DecodeAlias("id")]
    [Include]
    public string Id { get; internal set; }
    
    /// <summary>
    /// Name of the mod
    /// </summary>
    [DecodeAlias("name")]
    [Include]
    public string Name { get; internal set; }
    
    /// <summary>
    /// Author of the mod
    /// </summary>
    [DecodeAlias("author")]
    [Include]
    public string Author { get; internal set; }
    
    /// <summary>
    /// Version of the mod
    /// </summary>
    [DecodeAlias("version")]
    [Include]
    public string Version { get; internal set; }
    
    /// <summary>
    /// Description of the mod
    /// </summary>
    [DecodeAlias("description")]
    [Include]
    public string Description { get; internal set; }
    
    /// <summary>
    /// Game version the mod is compatible with
    /// </summary>
    [DecodeAlias("gameVersion")]
    [Include]
    public string GameVersion { get; internal set; }

    /// <summary>
    /// Where this mod is able to run. Possible values: "Client", "Server", "Universal".
    /// </summary>
    [DecodeAlias("platform")]
    [Include]
    public string Platform { get; internal set; } = "Client";
    
    /// <summary>
    /// Optional. List of dependencies of the mod
    /// </summary>
    [DecodeAlias("dependencies")]
    [Include]
    public string[] Dependencies { get; internal set; }
    
    /// <summary>
    /// Optional. The hex string color in which the mod's name will be displayed in the console
    /// </summary>
    /// <example>#ffffff</example>
    [DecodeAlias("logColor")]
    [Include]
    public string LogColor { get; internal set; }
    
    /// <summary>
    /// Optional. If the mods harmony patches shouldn't be applied automatically
    /// </summary>
    [DecodeAlias("dontApplyPatches")]
    [Include]
    public bool DontApplyPatches { get; internal set; }
    
    /// <summary>
    /// Optional. Download url of the mod.
    /// </summary>
    [DecodeAlias("url")]
    [Include]
    public string Url { get; internal set; }
    
    /// <summary>
    /// Optional. Priority of the mod.
    /// </summary>
    [DecodeAlias("priority")]
    [Include]
    public int Priority { get; internal set; }
    
    public Version VersionObject { get; internal set; }
}