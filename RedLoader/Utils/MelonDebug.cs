using System;
using System.Drawing;
using RedLoader.Utils;

namespace RedLoader
{
    public static class MelonDebug
    {
        public static void Msg(object obj)
        {
            if (!IsEnabled())
                return;
            RLog.Internal_Msg(Color.CornflowerBlue, RLog.DefaultTextColor, "DEBUG", obj.ToString());
            MsgCallbackHandler?.Invoke(LoggerUtils.DrawingColorToConsoleColor(RLog.DefaultTextColor), obj.ToString());
        }

        public static void Msg(string txt)
        {
            if (!IsEnabled())
                return;
            RLog.Internal_Msg(Color.CornflowerBlue, RLog.DefaultTextColor, "DEBUG", txt);
            MsgCallbackHandler?.Invoke(LoggerUtils.DrawingColorToConsoleColor(RLog.DefaultTextColor), txt);
        }

        public static void Msg(string txt, params object[] args)
        {
            if (!IsEnabled())
                return;
            RLog.Internal_Msg(Color.CornflowerBlue, RLog.DefaultTextColor, "DEBUG", string.Format(txt, args));
            MsgCallbackHandler?.Invoke(LoggerUtils.DrawingColorToConsoleColor(RLog.DefaultTextColor), string.Format(txt, args));
        }

        public static void Error(string txt)
        {
            if (!IsEnabled())
                return;
            RLog.Internal_Error("DEBUG", txt);
            ErrorCallbackHandler?.Invoke(txt);
        }

        public static event Action<ConsoleColor, string> MsgCallbackHandler;

        public static event Action<string> ErrorCallbackHandler;
        //public static bool IsEnabled() => MelonLaunchOptions.Core.DebugMode;

        public static bool IsEnabled()
        {
#if DEBUG
            return true;
#else
            return LaunchOptions.Core.IsDebug;
#endif
        }
    }
}