using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RedLoader.Utils;

public class SplashWindow
{
    [DllImport("ImGuiWindow.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int create_window();
    
    [DllImport("ImGuiWindow.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int close_window();
    
    [DllImport("ImGuiWindow.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void print_to_console(StringBuilder str);
    
    [DllImport("ImGuiWindow.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void load_background(StringBuilder str);
    
    [DllImport("ImGuiWindow.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void set_progress(float progress);
    
    [DllImport("ImGuiWindow.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void show_console(bool show);

    public static int TotalProgressSteps = 100;

    public static void CreateWindow()
    {
        var thread = new Thread(CreateWindowThread);
        thread.Start();
        
        Thread.Sleep(100);
        LoadBackground(Path.Combine(LoaderEnvironment.LoaderDirectory, "bg.png"));
    }
    
    public static void CloseWindow()
    {
        close_window();
    }
    
    public static void PrintToConsole(string str)
    {
        var sb = new StringBuilder(str);
        print_to_console(sb);
    }
    
    public static void SetProgress(float progress)
    {
        set_progress(progress);
    }
    
    public static void LoadBackground(string path)
    {
        var sb = new StringBuilder(path);
        load_background(sb);
    }

    public static void SetProgressSteps(int step)
    {
        set_progress(step / (float)TotalProgressSteps);
    }

    public static void HookLog()
    {
        RLog.MsgDrawingCallbackHandler -= LogCallback;
        RLog.MsgDrawingCallbackHandler += LogCallback;
    }
    
    public static void UnhookLog()
    {
        RLog.MsgDrawingCallbackHandler -= LogCallback;
    }

    private static void LogCallback(Color namesectionColor, Color textColor, string namesection, string text)
    {
        var sb = new StringBuilder();
        sb.Append($"[{namesection}] ");
        sb.Append(text);
        print_to_console(sb);
    }

    private static void CreateWindowThread()
    {
        create_window();
        show_console(true);
    }
}