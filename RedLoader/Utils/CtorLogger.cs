using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace RedLoader.Utils;

public class CtorLogger
{
    private static TextWriter _tickWriter;

    public static void Initialize(IEnumerable<AssemblyDefinition> assemblies)
    {
        var filename = Path.GetFullPath($"cctors_{DateTime.Now.Ticks}.log");
        Trace.TraceInformation($"Writing cctor dump to {filename}");

        StreamWriter fs = File.CreateText(filename);
        fs.AutoFlush = true;
        _tickWriter = fs;
        
        foreach (var assembly in assemblies)
        {
            Patch(assembly);
        }
    }

    private static void Patch(AssemblyDefinition ass)
    {
        MethodInfo dump = typeof(CtorLogger).GetMethod("DumpInfo", BindingFlags.Static | BindingFlags.Public);

        foreach (TypeDefinition typeDefinition in ass.MainModule.Types)
        {
            MethodDefinition cctor = typeDefinition.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
            ILProcessor il;
            if (cctor == null)
            {
                cctor = new MethodDefinition(".cctor",
                    MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig
                    | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    ass.MainModule.ImportReference(typeof(void)));
                typeDefinition.Methods.Add(cctor);
                il = cctor.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ret));
            }

            Instruction ins = cctor.Body.Instructions.First();
            il = cctor.Body.GetILProcessor();
            il.InsertBefore(ins, il.Create(OpCodes.Ldstr, typeDefinition.FullName));
            il.InsertBefore(ins, il.Create(OpCodes.Call, ass.MainModule.ImportReference(dump)));
        }
    }

    public static void DumpInfo(string typeName)
    {
        _tickWriter.WriteLine($"{typeName}");
    }
}