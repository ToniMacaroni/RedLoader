using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod;
using MonoMod.Utils;
using EventAttributes = Mono.Cecil.EventAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

// Man, I just want these warnings gone. This needs to be entirely rewritten anyway.
#pragma warning disable CA1051 // Do not declare visible instance fields

namespace RedLoader.Utils;

public class HookGeneratorV2 {
    static readonly Dictionary<Type, string> ReflTypeNameMap = new Dictionary<Type, string> () {
        { typeof(string), "string" },
        { typeof(object), "object" },
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(char), "char" },
        { typeof(decimal), "decimal" },
        { typeof(double), "double" },
        { typeof(short), "short" },
        { typeof(int), "int" },
        { typeof(long), "long" },
        { typeof(sbyte), "sbyte" },
        { typeof(float), "float" },
        { typeof(ushort), "ushort" },
        { typeof(uint), "uint" },
        { typeof(ulong), "ulong" },
        { typeof(void), "void" }
    };
    static readonly Dictionary<string, string> TypeNameMap = new Dictionary<string, string>();

    static HookGeneratorV2() {
        foreach (KeyValuePair<Type, string> pair in ReflTypeNameMap)
            TypeNameMap[pair.Key.FullName] = pair.Value;
    }

    public ModuleDefinition OutputModule;
    public ModuleDefinition OriginalModule;


    public string Namespace = "On";
    public bool HookOrig = true;
    public bool HookPrivate = true;
    public string HookExtName;

    public ModuleDefinition module_RedLoader;

    public TypeReference t_MulticastDelegate;
    public TypeReference t_IAsyncResult;
    public TypeReference t_AsyncCallback;
    public TypeReference t_MethodBase;
    public TypeReference t_RuntimeMethodHandle;
    public TypeReference t_EditorBrowsableState;

    public MethodReference m_Object_ctor;
    public MethodReference m_ObsoleteAttribute_ctor;
    public MethodReference m_EditorBrowsableAttribute_ctor;

    public MethodReference m_GetMethodFromHandle;
    private TypeReference t_Action;
    private readonly MethodReference m_AddPrefix;
    private readonly MethodReference m_Remove;
    private readonly MethodReference m_AddPostfix;
    private MethodReference m_GetTypeFromHandle;
    private TypeDefinition t_AccessTools;

    public HookGeneratorV2(ModuleDefinition originalModule, ModuleDefinition outputModule)
    {
        OriginalModule = originalModule;
        OutputModule = outputModule;
        // modder.MapDependency(modder.Module, "RedLoader");
        // if (!modder.DependencyCache.TryGetValue("RedLoader", out module_RedLoader))
        //     throw new FileNotFoundException("RedLoader not found!");
        
        // add directory of RedLoader.dll to assembly resolver
        var resolver = outputModule.AssemblyResolver as BaseAssemblyResolver ?? new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.Combine(LoaderEnvironment.LoaderDirectory, "net6"));
        
        var mscorlib = ModuleDefinition.ReadModule(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll");
        var il2cppSystem = ModuleDefinition.ReadModule(Path.Combine(LoaderEnvironment.DependenciesDirectory, "Il2CppSystem.dll"));

        //t_MulticastDelegate = OutputModule.ImportReference(ass_netstandard.GetType("System.MulticastDelegate"));

        t_MulticastDelegate = OutputModule.ImportReference(mscorlib.GetType("System.MulticastDelegate"));
        //t_MulticastDelegate = OutputModule.ImportReference(typeof(MulticastDelegate));
        t_IAsyncResult = OutputModule.ImportReference(mscorlib.GetType("System.IAsyncResult"));
        t_AsyncCallback = OutputModule.ImportReference(mscorlib.GetType("System.AsyncCallback"));
        t_MethodBase = OutputModule.ImportReference(mscorlib.GetType("System.Reflection.MethodBase"));
        t_RuntimeMethodHandle = OutputModule.ImportReference(mscorlib.GetType("System.RuntimeMethodHandle"));
        // t_EditorBrowsableState = OutputModule.ImportReference(typeof(Il2CppSystem.ComponentModel.EditorBrowsableState));
        t_EditorBrowsableState = OutputModule.ImportReference(il2cppSystem.GetType("Il2CppSystem.ComponentModel.EditorBrowsableState"));
        t_Action = OutputModule.ImportReference(mscorlib.GetType("System.Action"));
        m_GetTypeFromHandle = OutputModule.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"));
        t_AccessTools = OutputModule.ImportReference(typeof(HarmonyLib.AccessTools)).Resolve();
        
        // TypeDefinition td_HookManager = module_RedLoader.GetType("RedLoader.Utils.HookManager");
        TypeDefinition td_HookManager = outputModule.ImportReference(typeof(HookManager)).Resolve();

        m_Object_ctor = OutputModule.ImportReference(mscorlib.GetType("System.Object").Resolve().FindMethod("System.Void .ctor()"));
        m_ObsoleteAttribute_ctor = OutputModule.ImportReference(mscorlib.GetType("System.ObsoleteAttribute").Resolve().FindMethod("System.Void .ctor(System.String,System.Boolean)"));
        m_EditorBrowsableAttribute_ctor = OutputModule.ImportReference(il2cppSystem.GetType("Il2CppSystem.ComponentModel.EditorBrowsableAttribute").Resolve().FindMethod("System.Void .ctor(Il2CppSystem.ComponentModel.EditorBrowsableState)"));

        m_GetMethodFromHandle = OutputModule.ImportReference(
            new MethodReference("GetMethodFromHandle", t_MethodBase, t_MethodBase) {
                Parameters = {
                    new ParameterDefinition(t_RuntimeMethodHandle)
                }
            }
        );
        
        m_AddPrefix = OutputModule.ImportReference(td_HookManager.FindMethod("AddPrefix"));
        m_AddPostfix = OutputModule.ImportReference(td_HookManager.FindMethod("AddPostfix"));
        m_Remove = OutputModule.ImportReference(td_HookManager.FindMethod("Remove"));
    }

    // AccessTools.Method(typeof(Type), "MethodName", null, null)
    public void EmitAccessIl(ILProcessor il, ModuleDefinition module, TypeReference t, string name)
    {
        il.Emit(OpCodes.Ldtoken, t);

        // IL_0006: call class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)
        var typeFromHandleMethod = m_GetTypeFromHandle;
        il.Emit(OpCodes.Call, typeFromHandleMethod);

        // IL_000b: ldstr "MethodName"
        il.Emit(OpCodes.Ldstr, name);

        // IL_0010: ldnull
        il.Emit(OpCodes.Ldnull);

        // IL_0011: ldnull
        il.Emit(OpCodes.Ldnull);

        // IL_0012: call class [System.Runtime]System.Reflection.MethodInfo ['0Harmony']HarmonyLib.AccessTools::Method(class [System.Runtime]System.Type, string, class [System.Runtime]System.Type[], class [System.Runtime]System.Type[])
        il.Emit(OpCodes.Call, module.ImportReference(t_AccessTools.Methods.First(m => m.Name == "Method" && m.Parameters.Count == 4)));
    }

    public void Generate() {
        foreach (TypeDefinition type in OutputModule.Types) {
            GenerateFor(type, out TypeDefinition hookType);
            if (hookType == null || hookType.IsNested)
                continue;
            //OutputModule.Types.Add(hookType);
            type.NestedTypes.Add(hookType);
        }
    }

    public void GenerateFor(TypeDefinition type, out TypeDefinition hookType) {
        hookType = null;

        if (type.HasGenericParameters ||
            type.IsRuntimeSpecialName ||
            type.Name.StartsWith("<", StringComparison.Ordinal))
            return;

        if (!HookPrivate && type.IsNotPublic)
            return;

        //RLog.Msg($"[HookGen] Generating for type {type.FullName}");

        hookType = new TypeDefinition(
            //type.IsNested ? null : (Namespace + (string.IsNullOrEmpty(type.Namespace) ? "" : ("." + type.Namespace))),
            null,
            "Hooks",
            //(type.IsNested ? TypeAttributes.NestedPublic : TypeAttributes.Public) | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class,
            TypeAttributes.NestedPublic | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class,
            OutputModule.TypeSystem.Object
        );

        var add = false;

        foreach (MethodDefinition method in type.Methods)
        {
            try
            {
                add |= GenerateFor(hookType, method);
            }
            catch (Exception e)
            {
                //Console.WriteLine($"[HookGen] Failed to generate hook for {method.Name}({string.Join(", ", method.Parameters.Select(p => p.ParameterType.Name))}): {e}");
            }
        }

        foreach (TypeDefinition nested in type.NestedTypes) {
            GenerateFor(nested, out TypeDefinition hookNestedType);
            if (hookNestedType == null)
                continue;
            add = true;
            hookType.NestedTypes.Add(hookNestedType);
        }

        if (!add) {
            hookType = null;
        }
    }

    public bool GenerateFor(TypeDefinition hookType, MethodDefinition method) {
        if (method.HasGenericParameters ||
            method.IsConstructor ||
            method.IsAbstract ||
            method is { IsSpecialName: true, IsConstructor: false })
            return false;

        if (!HookOrig && method.Name.StartsWith("orig_", StringComparison.Ordinal))
            return false;
        if (!HookPrivate && method.IsPrivate)
            return false;

        var name = GetFriendlyName(method);
        bool suffix = method.Parameters.Count != 0;

        List<MethodDefinition> overloads = null;
        if (suffix) {
            overloads = method.DeclaringType.Methods.Where(other => !other.HasGenericParameters && GetFriendlyName(other) == name && other != method).ToList();
            if (!overloads.Any()) {
                suffix = false;
            }
        }

        if (suffix) {
            var builder = new StringBuilder();
            for (var parami = 0; parami < method.Parameters.Count; parami++) {
                ParameterDefinition param = method.Parameters[parami];
                if (!TypeNameMap.TryGetValue(param.ParameterType.FullName, out var typeName))
                    typeName = GetFriendlyName(param.ParameterType, false);

                if (overloads.Any(other => {
                        ParameterDefinition otherParam = other.Parameters.ElementAtOrDefault(parami);
                        return
                            otherParam != null &&
                            GetFriendlyName(otherParam.ParameterType, false) == typeName &&
                            otherParam.ParameterType.Namespace != param.ParameterType.Namespace;
                    }))
                    typeName = GetFriendlyName(param.ParameterType, true);

                builder.Append('_');
                builder.Append(typeName.Replace(".", "", StringComparison.Ordinal).Replace("`", "", StringComparison.Ordinal));
            }
            name += builder.ToString();
        }

        if (hookType.FindEvent(name) != null) {
            string nameTmp;
            for (
                var i = 1;
                hookType.FindEvent(nameTmp = name + "_" + i) != null;
                i++
            )
            { }

            name = nameTmp;
        }

        ILProcessor il;

        //MethodReference methodRef = OutputModule.ImportReference(method);

        #region Hook

        {
            TypeDefinition delHook = GenerateDelegateFor(method, false);
            delHook.Name = "hook_" + name;
            delHook.CustomAttributes.Add(GenerateEditorBrowsable(EditorBrowsableState.Never));
            hookType.NestedTypes.Add(delHook);
            
            var addHook = new MethodDefinition(
                "add_" + name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                OutputModule.TypeSystem.Void
            );
            addHook.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, delHook));
            addHook.Body = new MethodBody(addHook);
            il = addHook.Body.GetILProcessor();
            EmitAccessIl(il, OutputModule, OutputModule.ImportReference(method.DeclaringType), method.Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, m_AddPostfix);
            il.Emit(OpCodes.Ret);
            hookType.Methods.Add(addHook);

            var removeHook = new MethodDefinition(
                "remove_" + name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                OutputModule.TypeSystem.Void
            );
            removeHook.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, delHook));
            removeHook.Body = new MethodBody(removeHook);
            il = removeHook.Body.GetILProcessor();
            EmitAccessIl(il, OutputModule, OutputModule.ImportReference(method.DeclaringType), method.Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, m_Remove);
            il.Emit(OpCodes.Ret);
            hookType.Methods.Add(removeHook);

            var evHook = new EventDefinition(name, EventAttributes.None, delHook) {
                AddMethod = addHook,
                RemoveMethod = removeHook
            };
            hookType.Events.Add(evHook);
        }

        #endregion
        
        {
            var prefixName = name + "Prefix";
            
            TypeDefinition delHook = GenerateDelegateFor(method, true);
            delHook.Name = "hook_" + prefixName;
            delHook.CustomAttributes.Add(GenerateEditorBrowsable(EditorBrowsableState.Never));
            hookType.NestedTypes.Add(delHook);
            
            var addHook = new MethodDefinition(
                "add_" + prefixName,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                OutputModule.TypeSystem.Void
            );
            addHook.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, delHook));
            addHook.Body = new MethodBody(addHook);
            il = addHook.Body.GetILProcessor();
            EmitAccessIl(il, OutputModule, OutputModule.ImportReference(method.DeclaringType), method.Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, m_AddPrefix);
            il.Emit(OpCodes.Ret);
            hookType.Methods.Add(addHook);

            var removeHook = new MethodDefinition(
                "remove_" + prefixName,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                OutputModule.TypeSystem.Void
            );
            removeHook.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, delHook));
            removeHook.Body = new MethodBody(removeHook);
            il = removeHook.Body.GetILProcessor();
            EmitAccessIl(il, OutputModule, OutputModule.ImportReference(method.DeclaringType), method.Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, m_Remove);
            il.Emit(OpCodes.Ret);
            hookType.Methods.Add(removeHook);

            var evHook = new EventDefinition(prefixName, EventAttributes.None, delHook) {
                AddMethod = addHook,
                RemoveMethod = removeHook
            };
            hookType.Events.Add(evHook);
        }

        return true;
    }

    public TypeDefinition GenerateDelegateFor(MethodDefinition method, bool boolRet) {
        var name = GetFriendlyName(method);
        var index = method.DeclaringType.Methods.Where(other => !other.HasGenericParameters && GetFriendlyName(other) == name).ToList().IndexOf(method);
        if (index != 0) {
            var suffix = index.ToString(CultureInfo.InvariantCulture);
            do {
                name = name + "_" + suffix;
            } while (method.DeclaringType.Methods.Any(other => !other.HasGenericParameters && GetFriendlyName(other) == (name + suffix)));
        }
        name = "d_" + name;

        var del = new TypeDefinition(
            null, null,
            TypeAttributes.NestedPublic | TypeAttributes.Sealed | TypeAttributes.Class,
            t_MulticastDelegate
        );

        var ctor = new MethodDefinition(
            ".ctor",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.ReuseSlot,
            OutputModule.TypeSystem.Void
        ) {
            ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed,
            HasThis = true
        };
        ctor.Parameters.Add(new ParameterDefinition(OutputModule.TypeSystem.Object));
        ctor.Parameters.Add(new ParameterDefinition(OutputModule.TypeSystem.IntPtr));
        ctor.Body = new MethodBody(ctor);
        del.Methods.Add(ctor);

        var invoke = new MethodDefinition(
            "Invoke",
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
            /* ImportVisible(method.ReturnType) */ boolRet ? OutputModule.TypeSystem.Boolean : OutputModule.TypeSystem.Void
        ) {
            ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed,
            HasThis = true
        };
        if (!method.IsStatic) {
            TypeReference selfType = ImportVisible(method.DeclaringType);
            if (method.DeclaringType.IsValueType)
                selfType = new ByReferenceType(selfType);
            invoke.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, selfType));
        }
        foreach (ParameterDefinition param in method.Parameters)
            invoke.Parameters.Add(new ParameterDefinition(
                param.Name,
                param.Attributes & ~ParameterAttributes.Optional & ~ParameterAttributes.HasDefault,
                ImportVisible(param.ParameterType)
            ));
        invoke.Body = new MethodBody(invoke);
        del.Methods.Add(invoke);

        var invokeBegin = new MethodDefinition(
            "BeginInvoke",
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
            t_IAsyncResult
        ) {
            ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed,
            HasThis = true
        };
        foreach (ParameterDefinition param in invoke.Parameters)
            invokeBegin.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
        invokeBegin.Parameters.Add(new ParameterDefinition("callback", ParameterAttributes.None, t_AsyncCallback));
        invokeBegin.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, OutputModule.TypeSystem.Object));
        invokeBegin.Body = new MethodBody(invokeBegin);
        del.Methods.Add(invokeBegin);

        var invokeEnd = new MethodDefinition(
            "EndInvoke",
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
            OutputModule.TypeSystem.Object
        ) {
            ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed,
            HasThis = true
        };
        invokeEnd.Parameters.Add(new ParameterDefinition("result", ParameterAttributes.None, t_IAsyncResult));
        invokeEnd.Body = new MethodBody(invokeEnd);
        del.Methods.Add(invokeEnd);

        return del;
    }

    static string GetFriendlyName(MethodReference method) {
        var name = method.Name;
        if (name.StartsWith(".", StringComparison.Ordinal))
            name = name.Substring(1);
        name = name.Replace('.', '_');
        return name;
    }

    string GetFriendlyName(TypeReference type, bool full) {
        if (type is TypeSpecification) {
            var builder = new StringBuilder();
            BuildFriendlyName(builder, type, full);
            return builder.ToString();
        }

        return full ? type.FullName : type.Name;
    }
    void BuildFriendlyName(StringBuilder builder, TypeReference type, bool full) {
        if (!(type is TypeSpecification)) {
            builder.Append((full ? type.FullName : type.Name).Replace("_", "", StringComparison.Ordinal));
            return;
        }

        if (type.IsByReference) {
            builder.Append("ref");
        } else if (type.IsPointer) {
            builder.Append("ptr");
        }

        BuildFriendlyName(builder, ((TypeSpecification) type).ElementType, full);

        if (type.IsArray) {
            builder.Append("Array");
        }
    }

    static bool IsPublic(TypeDefinition typeDef) {
        return typeDef != null && (typeDef.IsNestedPublic || typeDef.IsPublic) && !typeDef.IsNotPublic;
    }

    bool HasPublicArgs(GenericInstanceType typeGen) {
        foreach (TypeReference arg in typeGen.GenericArguments) {
            // Generic parameter references are local.
            if (arg.IsGenericParameter)
                return false;

            if (arg is GenericInstanceType argGen && !HasPublicArgs(argGen))
                return false;

            if (!IsPublic(arg.SafeResolve()))
                return false;
        }

        return true;
    }

    TypeReference ImportVisible(TypeReference typeRef) {
        // Check if the declaring type is accessible.
        // If not, use its base type instead.
        // Note: This will break down with type specifications!
        TypeDefinition type = typeRef?.SafeResolve();
        goto Try;

        Retry:
        typeRef = type.BaseType;
        type = typeRef?.SafeResolve();

        Try:
        if (type == null) // Unresolvable - probably private anyway.
            return OutputModule.TypeSystem.Object;

        // Generic instance types are special. Try to match them exactly or baseify them.
        if (typeRef is GenericInstanceType typeGen && !HasPublicArgs(typeGen))
            goto Retry;

        // Check if the type and all of its parents are public.
        // Generic return / param types are too complicated at the moment and will be simplified.
        for (TypeDefinition parent = type; parent != null; parent = parent.DeclaringType) {
            if (IsPublic(parent) && (parent == type || !parent.HasGenericParameters))
                continue;
            // If it isn't public, ...
                
            if (type.IsEnum) {
                // ... try the enum's underlying type.
                typeRef = type.FindField("value__").FieldType;
                break;
            }

            // ... try the base type.
            goto Retry;
        }

        try {
            return OutputModule.ImportReference(typeRef);
        } catch {
            // Under rare circumstances, ImportReference can fail, f.e. Private<K> : Public<K, V>
            return OutputModule.TypeSystem.Object;
        }
    }

    CustomAttribute GenerateObsolete(string message, bool error) {
        var attrib = new CustomAttribute(m_ObsoleteAttribute_ctor);
        attrib.ConstructorArguments.Add(new CustomAttributeArgument(OutputModule.TypeSystem.String, message));
        attrib.ConstructorArguments.Add(new CustomAttributeArgument(OutputModule.TypeSystem.Boolean, error));
        return attrib;
    }

    CustomAttribute GenerateEditorBrowsable(EditorBrowsableState state) {
        var attrib = new CustomAttribute(m_EditorBrowsableAttribute_ctor);
        attrib.ConstructorArguments.Add(new CustomAttributeArgument(t_EditorBrowsableState, state));
        return attrib;
    }

}