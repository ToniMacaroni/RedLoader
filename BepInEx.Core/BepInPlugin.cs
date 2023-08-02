using System;

namespace BepInEx;

[AttributeUsage(AttributeTargets.Class)]
public class BepInPlugin : Attribute
{
    public BepInPlugin(string GUID, string Name, string Version)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.Version = Version;
    }

    public string GUID { get; protected set; }

    public string Name { get; protected set; }

    public string Version { get; protected set; }
}