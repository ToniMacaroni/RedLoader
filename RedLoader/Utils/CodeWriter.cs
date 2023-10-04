using System.IO;
using System.Text.RegularExpressions;

namespace RedLoader.Utils;

/// <summary>
/// Helper class for writing code to files
/// </summary>
public class CodeWriter
{
    private static readonly Regex _varNameRegex = new(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);

    public string Content { get; private set; }
    public int IndentLevel { get; private set; }

    public CodeWriter(string path)
    {
        _extension = Path.GetExtension(path);
        _path = path.Replace("extension", "");
        if (string.IsNullOrEmpty(_extension))
        {
            _extension = ".cs";
        }
    }
    
    public static string SanitizeVariableName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        string sanitized = input.Replace(' ', '_');

        if (char.IsDigit(sanitized[0]))
            sanitized = "_" + sanitized;

        sanitized = _varNameRegex.Replace(sanitized, string.Empty);

        return sanitized;
    }
    
    public CodeWriter Indent()
    {
        IndentLevel++;
        return this;
    }
    
    public CodeWriter Unindent()
    {
        IndentLevel--;
        return this;
    }

    public CodeWriter BlockStart()
    {
        Line("{");
        Indent();
        return this;
    }
    
    public CodeWriter BlockEnd()
    {
        Unindent();
        Line("}");
        return this;
    }

    public CodeWriter Line(string text)
    {
        for (var i = 0; i < IndentLevel; i++)
        {
            Content += "\t";
        }
        
        Content += text + "\n";
        return this;
    }

    public CodeWriter Line()
    {
        Content += "\n";
        return this;
    }

    public CodeWriter Add(string text)
    {
        Content += text;
        return this;
    }

    public CodeWriter Tab()
    {
        Content += "\t";
        return this;
    }

    public void Save()
    {
        File.WriteAllText(_path + _extension, Content);
    }

    private readonly string _path;
    private readonly string _extension;
}