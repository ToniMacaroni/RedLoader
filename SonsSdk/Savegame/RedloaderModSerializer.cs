using System.Diagnostics;
using System.Drawing;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using Newtonsoft.Json;
using RedLoader;
using RedLoader.Utils;
using Sons.Save;
using Object = Il2CppSystem.Object;
using CppString = Il2CppSystem.String;
using CppStringList = Il2CppSystem.Collections.Generic.List<Il2CppSystem.String>;
using JsonConvert = Alt.Json.JsonConvert;

namespace SonsSdk;

public class RedloaderModSerializer : Object
{
    public static RedloaderModSerializer Instance;

    private string _lastData;
    
    internal static void Init()
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

    internal static void RegisterSerializer()
    {
        var cs = new ISaveGameSerializer<ModSaveData>(Instance.Pointer);
        SaveGameManager.RegisterSerializer(cs);
    }
    
    public string SerializedName => "Mod";
    public bool UniqueFile => true;
    public bool ShouldSerialize => true;
    public bool IncludeInPlayerSave => true;
    public int ExecutionOrder => 6969;
    Il2CppSystem.Func<string, ModSaveData> DeserializeOverrideAction => (Il2CppSystem.Func<string, ModSaveData>)DeserializeOverride;
    
    ModSaveData OnSerialize()
    {
        RLog.Msg(Color.Yellow, new string('=', 50));
        RLog.Msg(Color.Yellow, "SERIALIZING MOD DATA");

        var sw = new Stopwatch();

        var datas = new System.Collections.Generic.List<NamedSaveData>();
        
        foreach (var (name, saveable) in SonsSaveTools.CustomSaveables)
        {
            try
            {
                var namedSaveData = new NamedSaveData();
                namedSaveData.Name = name;
                namedSaveData.Data = saveable.Serialize();
                datas.Add(namedSaveData);
                RLog.Msg(Color.Orange, $"Serializing {name} took {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            sw.Reset();
        }

        // var modSaveData = new ModSaveData(JSON.Dump(datas, EncodeOptions.NoTypeHints));
        var modSaveData = new ModSaveData(JsonConvert.SerializeObject(datas));
        sw.Stop();
        RLog.Msg(Color.Orange, $"Finishing up took {sw.ElapsedMilliseconds}ms");
        
        RLog.Msg(Color.Yellow, new string('=', 50));

        return modSaveData;
    }
    
    private ModSaveData DeserializeOverride(string data)
    {
        _lastData = data;
        return null;
    }

    // input data will be null here, so disregard it
    void OnDeserialize(ModSaveData _)
    {
        RLog.Msg(Color.Yellow, new string('=', 50));
        RLog.Msg(Color.Yellow, "DESERIALIZING MOD DATA");

        if (string.IsNullOrEmpty(_lastData))
        {
            RLog.Error("Could not deserialize mod data, no data was found.");
            return;
        }

        var sw = new Stopwatch();

        var data = SaveGameManager.Deserialize<ModSaveData>(_lastData);
        _lastData = null;

        RLog.Msg(Color.Orange, $"SaveGameManager deserializing took {sw.ElapsedMilliseconds}ms");
        sw.Reset();

        System.Collections.Generic.List<NamedSaveData> datas;
        
        try
        {
            // datas = JSON.Load(data.ModData.Get()).Make<System.Collections.Generic.List<NamedSaveData>>();
            datas = JsonConvert.DeserializeObject<System.Collections.Generic.List<NamedSaveData>>(data.ModData.Get());
        }
        catch (Exception e)
        {
            RLog.Error($"Error in deserializing mod data: {e}");
            throw;
        }

        RLog.Msg(Color.Orange, $"Constructing mod list took {sw.ElapsedMilliseconds}ms");
        sw.Reset();

        foreach (var modData in datas)
        {
            if (SonsSaveTools.CustomSaveables.TryGetValue(modData.Name, out var saveable))
            {
                try
                {
                    saveable.Deserialize(modData.Data);
                    RLog.Msg(Color.Orange, $"Deserializing {modData.Name} took {sw.ElapsedMilliseconds}ms");
                }
                catch (Exception e)
                {
                    RLog.Error($"Error deserializing {modData.Name}: {e}");
                    throw;
                }
                
                sw.Reset();
                continue;
            }
            
            RLog.Error($"Could not find saveable with name {modData.Name}");
        }
        
        sw.Stop();
        RLog.Msg(Color.Yellow, new string('=', 50));
    }

    public RedloaderModSerializer(IntPtr ptr) : base(ptr)
    { }

    public RedloaderModSerializer() : base(ClassInjector.DerivedConstructorPointer<RedloaderModSerializer>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
}
