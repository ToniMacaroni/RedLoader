using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class DocEntry
{
    public abstract EDocEntryType Type { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }

    public virtual string CreateTableEntry()
    {
        return string.Empty;
    }
}

public class ArgumentDocEntry : DocEntry
{
    public override EDocEntryType Type => EDocEntryType.Argument;
    public override string Name { get; }
    public override string Description { get; }
    
    public string Example { get; }

    public ArgumentDocEntry(string name, string description, string example = null)
    {
        Name = name;
        Description = description;
        Example = example;
    }

    public override string CreateTableEntry()
    {
        var argumentColumn = $"{Name}";
        var exampleColumn = "";
        if (!string.IsNullOrEmpty(Example))
            exampleColumn = $"{Example}";
        return $"| `{argumentColumn}` | `{exampleColumn}` | {Description} |";
    }
    
    public static string CreateDocTable(List<DocEntry> entries)
    {
        var stringBuilder = new StringBuilder();
    
        stringBuilder.AppendLine("| Argument | Example | Description |");
        stringBuilder.AppendLine("|:----------:|:---------:|:-------------:|");
    
        foreach (var entry in entries.Where(x=>x.Type == EDocEntryType.Argument))
        {
            stringBuilder.AppendLine(entry.CreateTableEntry());
        }

        return stringBuilder.ToString();
    }
}

public class ConfigDocEntry : DocEntry
{
    public override EDocEntryType Type => EDocEntryType.Config;
    public override string Name { get; }
    public override string Description { get; }
    
    public string ConfigType { get; }

    public ConfigDocEntry(string name, string description, string configType)
    {
        Name = name;
        Description = description;
        ConfigType = configType;
    }

    public override string CreateTableEntry()
    {
        var entryColumn = $"{Name}";
        var typeColumn = "";
        if (!string.IsNullOrEmpty(ConfigType))
            typeColumn = $"{ConfigType}";
        return $"| `{entryColumn}` | `{typeColumn}` | {Description} |";
    }
    
    public static string CreateDocTable(List<DocEntry> entries)
    {
        var stringBuilder = new StringBuilder();
    
        stringBuilder.AppendLine("| Entry | Type | Description |");
        stringBuilder.AppendLine("|:----------:|:---------:|:-------------:|");
    
        foreach (var entry in entries.Where(x=>x.Type == EDocEntryType.Config))
        {
            stringBuilder.AppendLine(entry.CreateTableEntry());
        }

        return stringBuilder.ToString();
    }
}

public class CommandDocEntry : DocEntry
{
    public override EDocEntryType Type => EDocEntryType.Command;
    public override string Name { get; }
    public override string Description { get; }
    
    public string Example { get; }

    public CommandDocEntry(string name, string description, string example = null)
    {
        Name = name;
        Description = description;
        Example = example;
    }

    public override string CreateTableEntry()
    {
        var commandColumn = $"{Name}";
        var exampleColumn = "";
        if (!string.IsNullOrEmpty(Example))
            exampleColumn = $"{Example}";
        return $"| `{commandColumn}` | `{exampleColumn}` | {Description} |";
    }
    
    public static string CreateDocTable(List<DocEntry> entries)
    {
        var stringBuilder = new StringBuilder();
    
        stringBuilder.AppendLine("| Command | Example | Description |");
        stringBuilder.AppendLine("|:----------:|:---------:|:-------------:|");
    
        foreach (var entry in entries.Where(x=>x.Type == EDocEntryType.Command))
        {
            stringBuilder.AppendLine(entry.CreateTableEntry());
        }

        return stringBuilder.ToString();
    }
}

public enum EDocEntryType
{
    Argument,
    Config,
    Command
}