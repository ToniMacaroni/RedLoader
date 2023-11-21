namespace SonsSdk;

public interface ICustomSaveable<T>
{
    string Name { get; }
    bool IncludeInPlayerSave { get; }
    T Save();
    void Load(T obj);
}