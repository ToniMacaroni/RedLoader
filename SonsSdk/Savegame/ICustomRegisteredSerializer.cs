namespace SonsSdk;

internal interface ICustomRegisteredSerializer
{
    string Name { get; }

    bool IncludeInPlayerSave { get; }

    bool Is(object serializer);

    string Serialize();

    void Deserialize(string json);

    bool IsType(Type type);

    bool IsSerializedType(Type type);
}