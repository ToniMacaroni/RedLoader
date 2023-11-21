using System.Drawing;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using RedLoader;
using Object = Il2CppSystem.Object;
using CppString = Il2CppSystem.String;
using CppRefString = Il2CppInterop.Runtime.InteropTypes.Fields.Il2CppReferenceField<Il2CppSystem.String>;

namespace SonsSdk;

public class ModSaveData : Object
{ 
    public CppRefString ModData;

    public ModSaveData(IntPtr ptr) : base(ptr)
    {
    }

    public ModSaveData(string modData) : base(ClassInjector.DerivedConstructorPointer<ModSaveData>())
    {
        ClassInjector.DerivedConstructorBody(this);
        
        ModData.Set(modData);
    }
}

public class NamedSaveData
{
    public string Name;
    public string Data;
}