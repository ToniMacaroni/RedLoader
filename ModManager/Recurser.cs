using MelonLoader;

namespace ModManager;

internal static class Recurser
{

    internal static void RecursiveFuncRun(RecursiveFuncVoid func)
    {
        if (func == null)
        {
            return;
        }

        func.Invoke(delegate { RecursiveFuncRun(func); });
    }

    internal delegate void RecursiveFuncRecurse();

    internal delegate void RecursiveFuncVoid(RecursiveFuncRecurse recurse);
}