namespace SonsSdk;

public class TooltipProvider
{
    private static bool _initialized;

    public static void Setup()
    {
        if (_initialized)
            return;

        _initialized = true;
    }
}