using Sons.Characters;
using TheForest;
using UnityEngine;

namespace SonsSdk;

public class GameManagers
{
    private static GameManagers _instance;
    private readonly Transform _gameManagersTr;

    internal GameManagers()
    {
        _gameManagersTr = DebugConsole.Instance.transform.parent;
    }

    internal static void Create()
    {
        _instance = new GameManagers();
    }

    internal static void Init()
    {
        SdkEvents.OnGameActivated.Subscribe(Create);
    }
    
    public static T GetManager<T>(string managerName) where T : Component
    {
        if (_instance == null)
        {
            return null;
        }
        
        if (!_instance._gameManagersTr)
            return null;
        
        return _instance._gameManagersTr.Find(managerName).GetComponent<T>();
    }

    public static CharacterManager GetCharacterManager() => GetManager<CharacterManager>("CharacterManager");
}