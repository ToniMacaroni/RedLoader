using System.IO;

namespace ForestNanosuit;

public class UFileWriter
{
    public string Content { get; private set; }

    public UFileWriter(string path)
    {
        _extension = Path.GetExtension(path);
        _path = path.Replace("extension", "");
        if (string.IsNullOrEmpty(_extension))
        {
            _extension = ".txt";
        }
    }

    public UFileWriter Line(string text)
    {
        Content += text + "\n";
        return this;
    }

    public UFileWriter Line()
    {
        Content += "\n";
        return this;
    }

    public UFileWriter Add(string text)
    {
        Content += text;
        return this;
    }

    public UFileWriter Tab()
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