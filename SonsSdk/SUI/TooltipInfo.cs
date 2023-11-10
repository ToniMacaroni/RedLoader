using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace SUI;

public class TooltipInfo : MonoBehaviour
{
    public string Text;
    
    static TooltipInfo()
    {
        ClassInjector.RegisterTypeInIl2Cpp<TooltipInfo>();
    }
}