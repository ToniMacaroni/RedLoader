namespace SonsSdk.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class DebugCommandAttribute : Attribute
{
    public string Command { get; }
    
    public DebugCommandAttribute(string command)
    {
        Command = command;
    }
}