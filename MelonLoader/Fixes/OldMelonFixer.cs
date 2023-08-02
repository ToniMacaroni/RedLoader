using System;
using System.Drawing;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using MelonLoader.Utils;

namespace MelonLoader.Fixes;

public class OldMelonFixer
{
    public static string Fix(string assemblyPath)
    {
        if (assemblyPath.EndsWith(".fixed.dll"))
            return assemblyPath;
        
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);

        if (!assemblyDefinition.HasCustomAttributes)
            return assemblyPath;

        foreach (var ca in assemblyDefinition.CustomAttributes)
        {
            if (ca.AttributeType.Name == "MelonInfoAttribute")
            {
                if (!assemblyDefinition.MainModule.AssemblyReferences.Any(x => x.Name.Contains("Il2CppSons")))
                {
                    return assemblyPath;
                }

                Process(assemblyDefinition);
                assemblyDefinition.Write(assemblyPath.Replace(".dll", ".fixed.dll"));
                assemblyDefinition.Dispose();
                Directory.CreateDirectory(Path.Combine(MelonEnvironment.ModsDirectory, "OldMods"));
                File.Move(assemblyPath, Path.Combine(MelonEnvironment.ModsDirectory, "OldMods", Path.GetFileName(assemblyPath)));
                return assemblyPath.Replace(".dll", ".fixed.dll");
            }
        }
        
        return assemblyPath;
    }
    
    public static void Process(AssemblyDefinition assemblyDefinition)
    {
        MelonLogger.Msg(ConsoleColor.Magenta, $"Fixing Old MelonLoader Assembly: {Path.GetFileName(assemblyDefinition.FullName)}");
        string oldNamespacePrefix = "Il2Cpp";

        foreach (var module in assemblyDefinition.Modules)
        {
            // Modifying Types used in Methods
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;

                    for (var i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        var instruction = method.Body.Instructions[i];

                        if (instruction.Operand is MethodReference methodReference)
                        {
                            if (methodReference.DeclaringType.Namespace.StartsWith(oldNamespacePrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                if(IsIl2CppSystemNamespace(methodReference.DeclaringType.Namespace))
                                    continue;
                                
                                var newNamespace = methodReference.DeclaringType.Namespace.Substring(oldNamespacePrefix.Length);

                                var oldAssemblyName = methodReference.DeclaringType.Scope.Name.Replace(".dll", "");
                                var newAssemblyName = oldAssemblyName.Replace(oldNamespacePrefix, "", StringComparison.OrdinalIgnoreCase);
                                var assemblyReference = module.AssemblyReferences.FirstOrDefault(a => a.Name.Equals(newAssemblyName, StringComparison.OrdinalIgnoreCase));

                                if (assemblyReference == null)
                                {
                                    assemblyReference = new AssemblyNameReference(newAssemblyName, new Version(0, 0, 0, 0));
                                    module.AssemblyReferences.Add(assemblyReference);
                                }

                                var oldAssemblyReference = module.AssemblyReferences.FirstOrDefault(a => a.Name.Equals(oldAssemblyName, StringComparison.OrdinalIgnoreCase));
                                if (oldAssemblyReference != null)
                                {
                                    module.AssemblyReferences.Remove(oldAssemblyReference);
                                }

                                var newType = new TypeReference(newNamespace, methodReference.DeclaringType.Name, module, assemblyReference);
                                var newMethod = new MethodReference(methodReference.Name, methodReference.ReturnType, newType)
                                {
                                    HasThis = methodReference.HasThis,
                                    ExplicitThis = methodReference.ExplicitThis,
                                    CallingConvention = methodReference.CallingConvention
                                };
                                foreach (var parameter in methodReference.Parameters)
                                {
                                    newMethod.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
                                }
                                method.Body.Instructions[i] = Instruction.Create(instruction.OpCode, newMethod);
                            }
                        }
                        else if (instruction.Operand is TypeReference typeReference)
                        {
                            if (typeReference.Namespace.StartsWith(oldNamespacePrefix))
                            {
                                if(IsIl2CppSystemNamespace(typeReference.Namespace))
                                    continue;
                                
                                var newNamespace = typeReference.Namespace.Substring(oldNamespacePrefix.Length);
                                var newType = module.ImportReference(new TypeReference(newNamespace, typeReference.Name, module, typeReference.Scope));
                                method.Body.Instructions[i] = Instruction.Create(instruction.OpCode, newType);
                            }
                        }
                    }
                }
            }

            // Modifying Types used in Fields
            foreach (var type in module.Types)
            {
                foreach (var field in type.Fields)
                {
                    if (field.FieldType.Namespace.StartsWith(oldNamespacePrefix))
                    {
                        if(IsIl2CppSystemNamespace(field.FieldType.Namespace))
                            continue;
                        
                        var newNamespace = field.FieldType.Namespace.Substring(oldNamespacePrefix.Length);
                        var newType = module.ImportReference(new TypeReference(newNamespace, field.FieldType.Name, module, field.FieldType.Scope));
                        field.FieldType = newType;
                    }
                }
            }

            // Modifying Types used in Attributes
            foreach (var type in module.Types)
            {
                for (int i = type.CustomAttributes.Count - 1; i >= 0; i--)
                {
                    var attr = type.CustomAttributes[i];
                    if (attr.AttributeType.Namespace.StartsWith(oldNamespacePrefix))
                    {
                        if(IsIl2CppSystemNamespace(attr.AttributeType.Namespace))
                            continue;
                            
                        var newNamespace = attr.AttributeType.Namespace.Substring(oldNamespacePrefix.Length);
                        var newType = module.ImportReference(new TypeReference(newNamespace, attr.AttributeType.Name, module, attr.AttributeType.Scope));
                        var newAttribute = new CustomAttribute(newType.Resolve().Methods.First(m => m.IsConstructor));

                        foreach (var arg in attr.ConstructorArguments)
                        {
                            newAttribute.ConstructorArguments.Add(arg);
                        }

                        foreach (var prop in attr.Properties)
                        {
                            newAttribute.Properties.Add(prop);
                        }

                        foreach (var field in attr.Fields)
                        {
                            newAttribute.Fields.Add(field);
                        }

                        type.CustomAttributes.RemoveAt(i);
                        type.CustomAttributes.Add(newAttribute);
                    }
                }
            }

            // Modifying Types used in Generic Parameters
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (method.HasGenericParameters)
                    {
                        foreach (var param in method.GenericParameters)
                        {
                            if (param.FullName.StartsWith(oldNamespacePrefix))
                            {
                                if(IsIl2CppSystemNamespace(param.FullName))
                                    continue;
                                
                                var newNamespace = param.FullName.Substring(oldNamespacePrefix.Length);
                                var newType = module.ImportReference(new TypeReference(newNamespace, param.Name, module, param.Scope));
                                param.Name = newType.Name;
                                param.Namespace = newType.Namespace;
                            }
                        }
                    }
                }
            }
        }
        
        MelonLogger.Msg("Fixed: " + assemblyDefinition.FullName);
    }

    private static bool IsIl2CppSystemNamespace(string ns)
    {
        var result = !ns.Contains("Sons") && !ns.Contains("Endnight");
        if(result)
        {
            MelonLogger.Warning($"Found Il2CppSystem Namespace: {ns}");
        }
        return result;
    }
}
