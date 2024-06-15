namespace SonsSdk.Attributes;

public interface IOnUpdateReceiver
{
    void OnUpdate();
}

public interface IOnInWorldUpdateReceiver
{
    void OnInWorldUpdate();
}

public interface IOnAfterSpawnReceiver
{
    void OnAfterSpawn();
}

public interface IOnGameActivatedReceiver
{
    void OnGameActivated();
}

public interface IOnBeforeLoadSaveReceiver
{
    void OnBeforeLoadSave();
}

public interface IOnAfterLoadSaveReceiver
{
    void OnAfterLoadSave();
}