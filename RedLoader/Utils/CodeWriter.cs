using System.IO;

namespace RedLoader.Utils;

/// <summary>
/// Helper class for writing code to files
/// </summary>
public class CodeWriter
{
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