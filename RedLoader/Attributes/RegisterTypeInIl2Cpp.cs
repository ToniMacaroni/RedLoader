﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Runtime.Injection;

namespace RedLoader
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterTypeInIl2Cpp : Attribute //Naming violation?
    {
        internal static List<Assembly> registrationQueue = new List<Assembly>();
        internal static bool ready;
        internal bool LogSuccess = true;

        public RegisterTypeInIl2Cpp() { }
        public RegisterTypeInIl2Cpp(bool logSuccess) { LogSuccess = logSuccess; }

        public static void RegisterAssembly(Assembly asm)
        {
            if (!ready)
            {
                registrationQueue.Add(asm);
                return;
            }

            IEnumerable<Type> typeTbl = asm.GetValidTypes();
            if ((typeTbl == null) || (typeTbl.Count() <= 0))
                return;
            foreach (Type type in typeTbl)
            {
                object[] attTbl = type.GetCustomAttributes(typeof(RegisterTypeInIl2Cpp), false);
                if ((attTbl == null) || (attTbl.Length <= 0))
                    continue;
                RegisterTypeInIl2Cpp att = (RegisterTypeInIl2Cpp)attTbl[0];
                if (att == null)
                    continue;
                ClassInjector.RegisterTypeInIl2Cpp(type, new RegisterTypeOptions(){LogSuccess = att.LogSuccess});
            }
        }

        internal static void SetReady()
        {
            ready = true;

            if (registrationQueue == null)
                return;

            foreach (var asm in registrationQueue)
                RegisterAssembly(asm);

            registrationQueue = null;
        }
    }
}
