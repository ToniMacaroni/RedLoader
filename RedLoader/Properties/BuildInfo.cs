namespace RedLoader
{
    public static class BuildInfo
    {
        public const string Name = "RedLoader";
        public const string Description = "RedLoader based on RedLoader";
        public const string Author = "Lava Gang & Toni Macaroni";
        public static readonly string Version;

        static BuildInfo()
        {
            var version = typeof(BuildInfo).Assembly.GetName().Version!;
            Version = $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}