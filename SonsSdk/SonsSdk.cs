namespace SonsSdk;

public static class SonsSdk
{
    public static void PrintMessage(string message, float duration = 3f)
    {
        if (!HudGui._instance)
            return;
        
        HudGui._instance.DisplayGeneralMessage(message, duration);
    }
}