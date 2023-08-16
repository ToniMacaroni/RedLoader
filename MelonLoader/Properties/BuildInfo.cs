using System;

namespace MelonLoader
{
    public static class BuildInfo
    {
        public const string Name = "SFLoader";
        public const string Description = "SFLoader based on MelonLoader";
        public const string Author = "Lava Gang & Toni Macaroni";
        public static readonly string Version;

        static BuildInfo()
        {
            var version = typeof(BuildInfo).Assembly.GetName().Version!;
            Version = $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}