using System.Drawing;
using RedLoader;
using RedLoader.TinyJSON;
using Sons.Gameplay.GameSetup;

namespace SonsSdk;

public static class SonsSaveTools
{
    internal static Dictionary<string, ICustomRegisteredSerializer> CustomSaveables = new();
    
    public static void Register<T>(ICustomSaveable<T> saveable)
    {
        CustomSaveables.Add(saveable.Name, new CustomRegisteredSerializers<T>(saveable));
    }
    
    public static void Unregister<T>(ICustomSaveable<T> saveable)
    {
        CustomSaveables.Remove(saveable.Name);
    }

    public static void Init()
    {
        RedloaderModSerializer.Init();
    }
}