﻿using System;
using System.Collections.Generic;
using System.Reflection;
using RedLoader.Unity.IL2CPP.Hook.Dobby;
using RedLoader.Unity.IL2CPP.Hook.Funchook;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using RedLoader;

namespace RedLoader.Unity.IL2CPP.Hook;

public interface INativeDetour : IDetour
{
    private static readonly DetourProvider DetourProviderType = DetourProvider.Default;

    public nint OriginalMethodPtr { get; }
    public nint DetourMethodPtr { get; }
    public nint TrampolinePtr { get; }

    private static INativeDetour CreateDefault<T>(nint original, T target) where T : Delegate =>
        // TODO: check and provide an OS accurate provider
        new DobbyDetour(original, target);

    public static INativeDetour Create<T>(nint original, T target) where T : Delegate
    {
        var detour = DetourProviderType switch
        {
            DetourProvider.Dobby    => new DobbyDetour(original, target),
            DetourProvider.Funchook => new FunchookDetour(original, target),
            _                       => CreateDefault(original, target)
        };
        
        return new CacheDetourWrapper(detour, target);
    }

    public static INativeDetour CreateAndApply<T>(nint from, T to, out T original)
        where T : Delegate
    {
        var detour = Create(from, to);
        original = detour.GenerateTrampoline<T>();
        detour.Apply();

        return detour;
    }

    // Workaround for CoreCLR collecting all delegates
    private class CacheDetourWrapper : INativeDetour
    {
        private readonly INativeDetour _wrapped;

        private List<object> _cache = new();

        public CacheDetourWrapper(INativeDetour wrapped, Delegate target)
        {
            _wrapped = wrapped;
            _cache.Add(target);
        }

        public void Dispose()
        {
            _wrapped.Dispose();
            _cache.Clear();
        }

        public void Apply() => _wrapped.Apply();

        public void Undo() => _wrapped.Undo();

        public void Free() => _wrapped.Free();

        public MethodBase GenerateTrampoline(MethodBase signature = null) => _wrapped.GenerateTrampoline(signature);

        public T GenerateTrampoline<T>() where T : Delegate
        {
            var trampoline = _wrapped.GenerateTrampoline<T>();
            _cache.Add(trampoline);
            return trampoline;
        }

        public bool IsValid => _wrapped.IsValid;

        public bool IsApplied => _wrapped.IsApplied;

        public nint OriginalMethodPtr => _wrapped.OriginalMethodPtr;

        public nint DetourMethodPtr => _wrapped.DetourMethodPtr;

        public nint TrampolinePtr => _wrapped.TrampolinePtr;
    }

    internal enum DetourProvider
    {
        Default,
        Dobby,
        Funchook
    }
}
