// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.Enumerable;
using System.Diagnostics.Internal;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace System.Diagnostics
{
    public partial class EnhancedStackTrace
    {
        private static readonly Type StackTraceHiddenAttributeType =
            Type.GetType("System.Diagnostics.StackTraceHiddenAttribute", false);

        private static List<EnhancedStackFrame> GetFrames(Exception exception)
        {
            if (exception == null) return new List<EnhancedStackFrame>();

            const bool needFileInfo = true;
            var stackTrace = new StackTrace(exception, needFileInfo);

            return GetFrames(stackTrace);
        }

        private static List<EnhancedStackFrame> GetFrames(StackTrace stackTrace)
        {
            var frames = new List<EnhancedStackFrame>();
            var stackFrames = stackTrace.GetFrames();

            if (stackFrames == null) return frames;

            for (var i = 0; i < stackFrames.Length; i++)
            {
                var frame = stackFrames[i];
                var method = frame.GetMethod();

                // Always show last stackFrame
                if (!ShowInStackTrace(method) && i < stackFrames.Length - 1) continue;
                var fileName = frame.GetFileName();
                var row = frame.GetFileLineNumber();
                var column = frame.GetFileColumnNumber();
                var stackFrame = new EnhancedStackFrame(frame, GetMethodDisplayString(method), fileName, row, column);
                frames.Add(stackFrame);
            }

            return frames;
        }

        public static ResolvedMethod GetMethodDisplayString(MethodBase originMethod)
        {
            // Special case: no method available
            if (originMethod == null) return null;

            var method = originMethod;

            var methodDisplayInfo = new ResolvedMethod
            {
                SubMethodBase = method
            };

            // Type name
            var type = method.DeclaringType;

            var subMethodName = method.Name;
            var methodName = method.Name;

            if (type != null && type.IsDefined(typeof(CompilerGeneratedAttribute), true) &&
                InteropHelper.Types.IAsyncStateMachine != null &&
                (InteropHelper.Types.IAsyncStateMachine.IsAssignableFrom(type) ||
                 typeof(IEnumerator).IsAssignableFrom(type)))
            {
                methodDisplayInfo.IsAsync = InteropHelper.Types.IAsyncStateMachine.IsAssignableFrom(type);

                // Convert StateMachine methods to correct overload +MoveNext()
                if (!TryResolveStateMachineMethod(ref method, out type))
                {
                    methodDisplayInfo.SubMethodBase = null;
                    subMethodName = null;
                }

                methodName = method.Name;
            }

            // Method name
            methodDisplayInfo.MethodBase = method;
            methodDisplayInfo.Name = methodName;
            if (method.Name.IndexOf("<", StringComparison.Ordinal) >= 0)
            {
                if (TryResolveGeneratedName(ref method, out type, out methodName, out subMethodName, out var kind,
                    out var ordinal))
                {
                    methodName = method.Name;
                    methodDisplayInfo.MethodBase = method;
                    methodDisplayInfo.Name = methodName;
                    methodDisplayInfo.Ordinal = ordinal;
                }
                else
                {
                    methodDisplayInfo.MethodBase = null;
                }

                methodDisplayInfo.IsLambda = kind == GeneratedNameKind.LambdaMethod;

                if (methodDisplayInfo.IsLambda && type != null)
                    if (methodName == ".cctor")
                    {
                        if (type.IsGenericTypeDefinition && !type.IsGenericType)
                        {
                            // TODO: diagnose type's generic type arguments from frame's "this" or something
                        }
                        else
                        {
                            var fields =
                                type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            foreach (var field in fields)
                            {
                                var value = field.GetValue(field);
                                if (value is Delegate d)
                                    if (ReferenceEquals(d.Method, originMethod) &&
                                        d.Target.ToString() == originMethod.DeclaringType?.ToString())
                                    {
                                        methodDisplayInfo.Name = field.Name;
                                        methodDisplayInfo.IsLambda = false;
                                        method = originMethod;
                                        break;
                                    }
                            }
                        }
                    }
            }

            if (subMethodName != methodName) methodDisplayInfo.SubMethod = subMethodName;

            // ResolveStateMachineMethod may have set declaringType to null
            if (type != null) methodDisplayInfo.DeclaringType = type;

            if (method is MethodInfo mi)
            {
                var returnParameter = mi.ReturnParameter;
                if (returnParameter != null)
                    methodDisplayInfo.ReturnParameter = GetParameter(mi.ReturnParameter);
                else if (mi.ReturnType != null)
                    methodDisplayInfo.ReturnParameter = new ResolvedParameter
                    {
                        Prefix = "",
                        Name = "",
                        ResolvedType = mi.ReturnType
                    };
            }

            if (method.IsGenericMethod)
            {
                var genericArguments = method.GetGenericArguments();
                var genericArgumentsString = string.Join(", ", genericArguments
                    .Select(arg => TypeNameHelper.GetTypeDisplayName(arg, false, true)).ToArray());
                methodDisplayInfo.GenericArguments += "<" + genericArgumentsString + ">";
                methodDisplayInfo.ResolvedGenericArguments = genericArguments;
            }

            // Method parameters
            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                var parameterList = new List<ResolvedParameter>(parameters.Length);
                foreach (var parameter in parameters) parameterList.Add(GetParameter(parameter));

                methodDisplayInfo.Parameters = parameterList;
            }

            if (methodDisplayInfo.SubMethodBase == methodDisplayInfo.MethodBase)
            {
                methodDisplayInfo.SubMethodBase = null;
            }
            else if (methodDisplayInfo.SubMethodBase != null)
            {
                parameters = methodDisplayInfo.SubMethodBase.GetParameters();
                if (parameters.Length > 0)
                {
                    var parameterList = new List<ResolvedParameter>(parameters.Length);
                    foreach (var parameter in parameters)
                    {
                        var param = GetParameter(parameter);
                        if (param.Name?.StartsWith("<") ?? true) continue;

                        parameterList.Add(param);
                    }

                    methodDisplayInfo.SubMethodParameters = parameterList;
                }
            }

            return methodDisplayInfo;
        }

        private static bool TryResolveGeneratedName(ref MethodBase method, out Type type, out string methodName,
            out string subMethodName, out GeneratedNameKind kind, out int? ordinal)
        {
            kind = GeneratedNameKind.None;
            type = method.DeclaringType;
            subMethodName = null;
            ordinal = null;
            methodName = method.Name;

            var generatedName = methodName;

            if (!TryParseGeneratedName(generatedName, out kind, out var openBracketOffset, out var closeBracketOffset))
                return false;

            methodName = generatedName.Substring(openBracketOffset + 1, closeBracketOffset - openBracketOffset - 1);

            switch (kind)
            {
                case GeneratedNameKind.LocalFunction:
                    {
                        var localNameStart = generatedName.IndexOf((char)kind, closeBracketOffset + 1);
                        if (localNameStart < 0) break;
                        localNameStart += 3;

                        if (localNameStart < generatedName.Length)
                        {
                            var localNameEnd = generatedName.IndexOf("|", localNameStart, StringComparison.Ordinal);
                            if (localNameEnd > 0)
                                subMethodName = generatedName.Substring(localNameStart, localNameEnd - localNameStart);
                        }

                        break;
                    }
                case GeneratedNameKind.LambdaMethod:
                    subMethodName = generatedName;
                    break;

                default:
                    break;
            }

            var dt = method.DeclaringType;
            if (dt == null) return false;

            var matchHint = GetMatchHint(kind, method);

            var matchName = methodName;

            var candidateMethods =
                dt.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                              BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.Name == matchName);
            if (TryResolveSourceMethod(candidateMethods.Cast<MethodBase>(), kind, matchHint, ref method, ref type,
                out ordinal)) return true;

            var candidateConstructors =
                dt.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                   BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.Name == matchName);
            if (TryResolveSourceMethod(candidateConstructors.Cast<MethodBase>(), kind, matchHint, ref method, ref type,
                out ordinal)) return true;

            const int MaxResolveDepth = 10;
            for (var i = 0; i < MaxResolveDepth; i++)
            {
                dt = dt.DeclaringType;
                if (dt == null) return false;

                candidateMethods =
                    dt.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                  BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.Name == matchName);
                if (TryResolveSourceMethod(candidateMethods.Cast<MethodBase>(), kind, matchHint, ref method, ref type,
                    out ordinal)) return true;

                candidateConstructors =
                    dt.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                       BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(m => m.Name == matchName);
                if (TryResolveSourceMethod(candidateConstructors.Cast<MethodBase>(), kind, matchHint, ref method,
                    ref type, out ordinal)) return true;

                if (methodName == ".cctor")
                {
                    candidateConstructors =
                        dt.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                           BindingFlags.DeclaredOnly).Where(m => m.Name == matchName);
                    foreach (var cctor in candidateConstructors)
                    {
                        method = cctor;
                        type = dt;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryResolveSourceMethod(IEnumerable<MethodBase> candidateMethods, GeneratedNameKind kind,
            string matchHint, ref MethodBase method, ref Type type, out int? ordinal)
        {
            ordinal = null;
            foreach (var candidateMethod in candidateMethods)
            {
                var methodBody = candidateMethod.GetMethodBody();
                if (kind == GeneratedNameKind.LambdaMethod)
                    foreach (var v in EnumerableIList.Create(methodBody?.LocalVariables))
                    {
                        if (v.LocalType == type) GetOrdinal(method, ref ordinal);
                        method = candidateMethod;
                        type = method.DeclaringType;
                        return true;
                    }

                try
                {
                    var rawIL = methodBody?.GetILAsByteArray();
                    if (rawIL == null) continue;
                    var reader = new ILReader(rawIL);
                    while (reader.Read(candidateMethod))
                        if (reader.Operand is MethodBase mb)
                            if (method == mb || matchHint != null && method.Name.Contains(matchHint))
                            {
                                if (kind == GeneratedNameKind.LambdaMethod) GetOrdinal(method, ref ordinal);

                                method = candidateMethod;
                                type = method.DeclaringType;
                                return true;
                            }
                }
                catch
                {
                    // https://github.com/benaadams/Ben.Demystifier/issues/32
                    // Skip methods where il can't be interpreted
                }
            }

            return false;
        }

        private static void GetOrdinal(MethodBase method, ref int? ordinal)
        {
            var lamdaStart = method.Name.IndexOf((char)GeneratedNameKind.LambdaMethod + "__", StringComparison.Ordinal) + 3;
            if (lamdaStart > 3)
            {
                var secondStart = method.Name.IndexOf("_", lamdaStart, StringComparison.Ordinal) + 1;
                if (secondStart > 0) lamdaStart = secondStart;

                if (!int.TryParse(method.Name.Substring(lamdaStart), out var foundOrdinal))
                {
                    ordinal = null;
                    return;
                }

                ordinal = foundOrdinal;

                var methods = method.DeclaringType?.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                              BindingFlags.Static | BindingFlags.Instance |
                                                              BindingFlags.DeclaredOnly);

                var count = 0;
                if (methods != null)
                {
                    var startName = method.Name.Substring(0, lamdaStart);
                    foreach (var m in methods)
                        if (m.Name.Length > lamdaStart && m.Name.StartsWith(startName))
                        {
                            count++;

                            if (count > 1) break;
                        }
                }
                if (count <= 1) ordinal = null;
            }
        }

        private static string GetMatchHint(GeneratedNameKind kind, MethodBase method)
        {
            var methodName = method.Name;

            switch (kind)
            {
                case GeneratedNameKind.LocalFunction:
                    var start = methodName.IndexOf("|", StringComparison.Ordinal);
                    if (start < 1) return null;
                    var end = methodName.IndexOf("_", start, StringComparison.Ordinal) + 1;
                    if (end <= start) return null;

                    return methodName.Substring(start, end - start);

                default:
                    break;
            }

            return null;
        }

        // Parse the generated name. Returns true for names of the form
        // [CS$]<[middle]>c[__[suffix]] where [CS$] is included for certain
        // generated names, where [middle] and [__[suffix]] are optional,
        // and where c is a single character in [1-9a-z]
        // (csharp\LanguageAnalysis\LIB\SpecialName.cpp).
        internal static bool TryParseGeneratedName(
            string name,
            out GeneratedNameKind kind,
            out int openBracketOffset,
            out int closeBracketOffset)
        {
            openBracketOffset = -1;
            if (name.StartsWith("CS$<", StringComparison.Ordinal))
                openBracketOffset = 3;
            else if (name.StartsWith("<", StringComparison.Ordinal)) openBracketOffset = 0;

            if (openBracketOffset >= 0)
            {
                closeBracketOffset = IndexOfBalancedParenthesis(name, openBracketOffset, '>');
                if (closeBracketOffset >= 0 && closeBracketOffset + 1 < name.Length)
                {
                    int c = name[closeBracketOffset + 1];
                    if (c >= '1' && c <= '9' || c >= 'a' && c <= 'z') // Note '0' is not special.
                    {
                        kind = (GeneratedNameKind)c;
                        return true;
                    }
                }
            }

            kind = GeneratedNameKind.None;
            openBracketOffset = -1;
            closeBracketOffset = -1;
            return false;
        }


        private static int IndexOfBalancedParenthesis(string str, int openingOffset, char closing)
        {
            var opening = str[openingOffset];

            var depth = 1;
            for (var i = openingOffset + 1; i < str.Length; i++)
            {
                var c = str[i];
                if (c == opening)
                {
                    depth++;
                }
                else if (c == closing)
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }

            return -1;
        }

        private static string GetPrefix(ParameterInfo parameter)
        {
            if (Attribute.IsDefined(parameter, typeof(ParamArrayAttribute), false)) return "params";

            if (parameter.IsOut) return "out";

            if (parameter.IsIn) return "in";

            if (parameter.ParameterType.IsByRef) return "ref";

            return string.Empty;
        }

        private static ResolvedParameter GetParameter(ParameterInfo parameter)
        {
            var prefix = GetPrefix(parameter);
            var parameterType = parameter.ParameterType;

            if (parameterType.IsGenericType)
            {
                var customAttribs = parameter.GetCustomAttributes(false);

                var tupleNameAttribute = customAttribs.OfType<Attribute>()
                    .FirstOrDefault(a => a.IsTupleElementNameAttribue());

                var tupleNames = tupleNameAttribute?.GetTransformerNames();

                if (tupleNames?.Count > 0)
                    return GetValueTupleParameter(tupleNames, prefix, parameter.Name, parameterType);
            }

            if (parameterType.IsByRef) parameterType = parameterType.GetElementType();

            return new ResolvedParameter
            {
                Prefix = prefix,
                Name = parameter.Name,
                ResolvedType = parameterType,
                IsDynamicType = InteropHelper.Types.DynamicAttribute != null &&
                                parameter.IsDefined(InteropHelper.Types.DynamicAttribute, false)
            };
        }

        private static ResolvedParameter GetValueTupleParameter(IList<string> tupleNames, string prefix, string name,
            Type parameterType)
        {
            return new ValueTupleResolvedParameter
            {
                TupleNames = tupleNames,
                Prefix = prefix,
                Name = name,
                ResolvedType = parameterType
            };
        }

        private static string GetValueTupleParameterName(IList<string> tupleNames, Type parameterType)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            var args = parameterType.GetGenericArguments();
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0) sb.Append(", ");

                sb.Append(TypeNameHelper.GetTypeDisplayName(args[i], false, true));

                if (i >= tupleNames.Count) continue;

                var argName = tupleNames[i];
                if (argName == null) continue;

                sb.Append(" ");
                sb.Append(argName);
            }

            sb.Append(")");
            return sb.ToString();
        }

        private static bool ShowInStackTrace(MethodBase method)
        {
            Debug.Assert(method != null);

            if (StackTraceHiddenAttributeType != null)
                // Don't show any methods marked with the StackTraceHiddenAttribute
                // https://github.com/dotnet/coreclr/pull/14652
                if (IsStackTraceHidden(method))
                    return false;

            var type = method.DeclaringType;

            if (type == null) return true;

            if (type == InteropHelper.Types.GenericTask && method.Name == "InnerInvoke") return false;
            if (type == InteropHelper.Types.GenericValueTask && method.Name == "get_Result") return false;
            if (type == InteropHelper.Types.Task)
                switch (method.Name)
                {
                    case "ExecuteWithThreadLocal":
                    case "Execute":
                    case "ExecutionContextCallback":
                    case "ExecuteEntry":
                    case "InnerInvoke":
                        return false;
                }

            if (type == typeof(ExecutionContext))
                switch (method.Name)
                {
                    case "RunInternal":
                    case "Run":
                        return false;
                }

            if (StackTraceHiddenAttributeType != null)
            {
                // Don't show any types marked with the StackTraceHiddenAttribute
                // https://github.com/dotnet/coreclr/pull/14652
                if (IsStackTraceHidden(type)) return false;
            }
            else
            {
                // Fallbacks for runtime pre-StackTraceHiddenAttribute
                if (type == InteropHelper.Types.ExceptionDispatchInfo && method.Name == "Throw")
                    return false;
                if (type == InteropHelper.Types.TaskAwaiter ||
                    type == InteropHelper.Types.GenericTaskAwaiter ||
                    type == InteropHelper.Types.GenericValueTaskAwaiter ||
                    type == InteropHelper.Types.ValueTaskAwaiter ||
                    type == InteropHelper.Types.GenericConfiguredValueTaskAwaitable_ConfiguredValueTaskAwaiter ||
                    type == InteropHelper.Types.ConfiguredValueTaskAwaitable_ConfiguredValueTaskAwaiter ||
                    type == InteropHelper.Types.GenericConfiguredTaskAwaitable_ConfiguredTaskAwaiter ||
                    type == InteropHelper.Types.ConfiguredTaskAwaitable_ConfiguredTaskAwaiter)
                    switch (method.Name)
                    {
                        case "HandleNonSuccessAndDebuggerNotification":
                        case "ThrowForNonSuccess":
                        case "ValidateEnd":
                        case "GetResult":
                            return false;
                    }
                else if (type.FullName == "System.ThrowHelper") return false;
            }

            return true;
        }

        private static bool IsStackTraceHidden(MemberInfo memberInfo)
        {
            if (!memberInfo.Module.Assembly.ReflectionOnly)
                return memberInfo.GetCustomAttributes(StackTraceHiddenAttributeType, false).Length != 0;

            EnumerableIList<object> attributes;
            try
            {
                attributes = EnumerableIList.Create(memberInfo.GetCustomAttributes(true));
            }
            catch (NotImplementedException)
            {
                return false;
            }

            return attributes.Any(attribute => attribute.GetType().FullName == StackTraceHiddenAttributeType.FullName);
        }

        private static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType)
        {
            Debug.Assert(method != null);
            Debug.Assert(method.DeclaringType != null);

            declaringType = method.DeclaringType;
            if (declaringType == null) return false;

            var parentType = declaringType.DeclaringType;
            if (parentType == null) return false;

            var methods = parentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                                BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (InteropHelper.Types.StateMachineAttribute != null &&
                InteropHelper.Types.IteratorStateMachineAttribute != null)
                foreach (var candidateMethod in methods)
                {
                    var attributes =
                        candidateMethod.GetCustomAttributes(InteropHelper.Types.StateMachineAttribute, true);

                    foreach (var asma in attributes)
                    {
                        var type = InteropHelper.Types.StateMachineType.GetValue(asma, null) as Type;
                        if (type == declaringType)
                        {
                            method = candidateMethod;
                            declaringType = candidateMethod.DeclaringType;
                            // Mark the iterator as changed; so it gets the + annotation of the original method
                            // async statemachines resolve directly to their builder methods so aren't marked as changed
                            return InteropHelper.Types.IteratorStateMachineAttribute.IsInstanceOfType(asma);
                        }
                    }
                }

            return false;
        }

        internal enum GeneratedNameKind
        {
            None = 0,

            // Used by EE:
            ThisProxyField = '4',
            HoistedLocalField = '5',
            DisplayClassLocalOrField = '8',
            LambdaMethod = 'b',
            LambdaDisplayClass = 'c',
            StateMachineType = 'd',

            LocalFunction =
                'g', // note collision with Deprecated_InitializerLocal, however this one is only used for method names

            // Used by EnC:
            AwaiterField = 'u',
            HoistedSynthesizedLocalField = 's',

            // Currently not parsed:
            StateMachineStateField = '1',
            IteratorCurrentBackingField = '2',
            StateMachineParameterProxyField = '3',
            ReusableHoistedLocalField = '7',
            LambdaCacheField = '9',
            FixedBufferField = 'e',
            AnonymousType = 'f',
            TransparentIdentifier = 'h',
            AnonymousTypeField = 'i',
            AutoPropertyBackingField = 'k',
            IteratorCurrentThreadIdField = 'l',
            IteratorFinallyMethod = 'm',
            BaseMethodWrapper = 'n',
            AsyncBuilderField = 't',
            DynamicCallSiteContainerType = 'o',
            DynamicCallSiteField = 'p'
        }
    }
}