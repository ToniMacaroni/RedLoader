using System.Collections;
using System.Collections.Generic;
using System.IO;
using RedLoader.TinyJSON;

namespace RedLoader.Utils;

public struct PathObject
{
    public string Path { get; private set; }
    
    public PathObject(string path) => Path = path;
    
    public bool FileExists() => File.Exists(Path);
    
    public bool DirectoryExists() => Directory.Exists(Path);

    public IEnumerable<string> GetFiles() => Directory.GetFiles(Path);
    
    public IEnumerable<string> GetFiles(string searchPattern) => Directory.GetFiles(Path, searchPattern);
    
    public string ReadText() => File.ReadAllText(Path);

    public T ReadJson<T>() => JSON.Load(ReadText()).Make<T>();
    
    public void WriteText(string text) => File.WriteAllText(Path, text);
    
    public void WriteJson(object obj) => WriteText(JSON.Dump(obj));

    public static PathObject operator /(PathObject path1, PathObject path2) => new(System.IO.Path.Combine(path1.Path, path2.Path));
    public static PathObject operator /(PathObject path1, string path2) => new(System.IO.Path.Combine(path1.Path, path2));
    
    public static implicit operator string(PathObject path) => path.Path;
}