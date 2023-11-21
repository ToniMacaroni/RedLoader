using System.Drawing;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using Newtonsoft.Json;
using RedLoader;
using RedLoader.TinyJSON;
using Sons.Save;
using Object = Il2CppSystem.Object;
using CppString = Il2CppSystem.String;
using CppStringList = Il2CppSystem.Collections.Generic.List<Il2CppSystem.String>;

namespace SonsSdk;

public class RedloaderModSerializer : Object
{
    public static RedloaderModSerializer Instance;

    private string _lastData;
    
    public static void Init()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ModSaveData>();
        ClassInjector.RegisterTypeInIl2Cpp<RedloaderModSerializer>(new()
        {
            LogSuccess = true,
            Interfaces = new[] { typeof(ISaveGameSerializer<ModSaveData>) }
        });
        
        Instance = new RedloaderModSerializer();

        RegisterSerializer();
    }

    public static void RegisterSerializer()
    {
        var cs = new ISaveGameSerializer<ModSaveData>(Instance.Pointer);
        SaveGameManager.RegisterSerializer(cs);
    }
    
    public string SerializedName => "Mod";
    public bool UniqueFile => true;
    public bool ShouldSerialize => true;
    public bool IncludeInPlayerSave => true;
    Il2CppSystem.Func<string, ModSaveData> DeserializeOverrideAction => (Il2CppSystem.Func<string, ModSaveData>)DeserializeOverride;
    
    ModSaveData OnSerialize()
    {
        RLog.Msg(Color.Orange, new string('=', 50));
        RLog.Msg(Color.Orange, "SERIALIZING MOD DATA");
        RLog.Msg(Color.Orange, new string('=', 50));

        var datas = new System.Collections.Generic.List<NamedSaveData>();
        
        foreach (var (name, saveable) in SonsSaveTools.CustomSaveables)
        {
            var namedSaveData = new NamedSaveData();
            namedSaveData.Name = name;
            namedSaveData.Data = saveable.Serialize();
            datas.Add(namedSaveData);
        }

        return new ModSaveData(JSON.Dump(datas, EncodeOptions.NoTypeHints));
    }
    
    private ModSaveData DeserializeOverride(string data)
    {
        _lastData = data;
        return null;
    }

    // input data will be null here, so disregard it
    void OnDeserialize(ModSaveData _)
    {
        RLog.Msg(Color.Orange, new string('=', 50));
        RLog.Msg(Color.Orange, "DESERIALIZING MOD DATA");
        RLog.Msg(Color.Orange, new string('=', 50));

        if (string.IsNullOrEmpty(_lastData))
        {
            RLog.Error("Could not deserialize mod data, no data was found.");
            return;
        }

        var data = SaveGameManager.Deserialize<ModSaveData>(_lastData);
        _lastData = null;

        var datas = JSON.Load(data.ModData.Get()).Make<System.Collections.Generic.List<NamedSaveData>>();
        
        foreach (var modData in datas)
        {
            if (SonsSaveTools.CustomSaveables.TryGetValue(modData.Name, out var saveable))
            {
                saveable.Deserialize(modData.Data);
                continue;
            }
            
            RLog.Error($"Could not find saveable with name {modData.Name}");
        }
    }

    public RedloaderModSerializer(IntPtr ptr) : base(ptr)
    { }

    public RedloaderModSerializer() : base(ClassInjector.DerivedConstructorPointer<RedloaderModSerializer>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
}