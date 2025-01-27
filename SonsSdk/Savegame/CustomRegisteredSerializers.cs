using Alt.Json;
using RedLoader;

namespace SonsSdk;

internal class CustomRegisteredSerializers<T> : ICustomRegisteredSerializer
{
    public ICustomSaveable<T> Serializer;

    public string Name => Serializer.Name;
    
    public bool IncludeInPlayerSave => Serializer.IncludeInPlayerSave;

    public bool Is(object serializer)
    {
        return Serializer == serializer;
    }

    public string Serialize()
    {
        // return JSON.Dump(Serializer.Save(), EncodeOptions.NoTypeHints);
        return LoaderUtils.JsonSerialize(Serializer.Save());
    }

    public void Deserialize(string json)
    {
        // var data = JSON.Load(json).Make<T>();
        var data = LoaderUtils.JsonDeserialize<T>(json);
        Serializer.Load(data);
    }

    public bool IsType(Type type)
    {
        return Serializer.GetType() == type;
    }

    public bool IsSerializedType(Type type)
    {
        return typeof(T) == type;
    }

    public CustomRegisteredSerializers(ICustomSaveable<T> serializer)
    {
        Serializer = serializer;
    }
}
