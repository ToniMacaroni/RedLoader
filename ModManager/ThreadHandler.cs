using MelonLoader;

namespace ModManager;

internal static class ThreadHandler
{
    internal static void RefreshReleases()
    {
        if (Releases.All.Count <= 0)
        {
            Releases.RequestLists();
        }
    }

    internal static void RecursiveFuncRun(RecursiveFuncVoid func)
    {
        if (func == null)
        {
            return;
        }

        func.Invoke(delegate { RecursiveFuncRun(func); });
    }

    private static void GetReleases()
    {
        RefreshReleases();
    }

    internal delegate void RecursiveFuncRecurse();

    internal delegate void RecursiveFuncVoid(RecursiveFuncRecurse recurse);
}