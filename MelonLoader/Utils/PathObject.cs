using System.IO;
using MelonLoader.TinyJSON;

namespace MelonLoader.Utils;

public struct PathObject
{
    public string Path { get; private set; }
    
    public PathObject(string path) => Path = path;
    
    public bool FileExists() => File.Exists(Path);
    
    public bool DirectoryExists() => Directory.Exists(Path);
    
    public string ReadText() => File.ReadAllText(Path);

    public T ReadJson<T>() => JSON.Load(ReadText()).Make<T>();
    
    public void WriteText(string text) => File.WriteAllText(Path, text);
    
    public void WriteJson(object obj) => WriteText(JSON.Dump(obj));

    public static PathObject operator /(PathObject path1, PathObject path2) => new(System.IO.Path.Combine(path1.Path, path2.Path));
}