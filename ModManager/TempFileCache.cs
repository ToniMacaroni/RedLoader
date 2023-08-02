using System.Collections.Generic;
using System.IO;

namespace MelonLoader;

internal static class TempFileCache
{
    private static readonly List<string> TempFiles = new();

    internal static string CreateFile()
    {
        var temppath = Path.GetTempFileName();
        TempFiles.Add(temppath);
        return temppath;
    }

    internal static void ClearCache()
    {
        if (TempFiles.Count <= 0)
        {
            return;
        }

        foreach (var file in TempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}