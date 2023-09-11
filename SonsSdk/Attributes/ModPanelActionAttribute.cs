namespace SonsSdk.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ModPanelActionAttribute : Attribute
{
    public string Name { get; }
    
    public ModPanelActionAttribute(string name)
    {
        Name = name;
    }
}