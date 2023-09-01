using System.IO;

namespace RedLoader.Utils;

/// <summary>
/// Helper class for writing text to files
/// </summary>
public class FileWriter
{
    public string Content { get; private set; }

    public FileWriter(string path)
    {
        _extension = Path.GetExtension(path);
        _path = path.Replace("extension", "");
        if (string.IsNullOrEmpty(_extension))
        {
            _extension = ".txt";
        }
    }

    public FileWriter Line(string text)
    {
        Content += text + "\n";
        return this;
    }

    public FileWriter Line()
    {
        Content += "\n";
        return this;
    }

    public FileWriter Add(string text)
    {
        Content += text;
        return this;
    }

    public FileWriter Tab()
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