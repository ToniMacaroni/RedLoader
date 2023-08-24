using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Mono.CSharp;
using UnityEngine;

namespace RedLoader.Utils;

internal static class MelonConsole
{
    private const int STD_OUTPUT_HANDLE = -11;
    
    internal static IntPtr ConsoleOutHandle = IntPtr.Zero;
    internal static FileStream ConsoleOutStream = null;
    internal static StreamWriter ConsoleOutWriter = null;
    
    internal static void Init()
    {
        if (MelonUtils.IsUnderWineOrSteamProton() || !MelonUtils.IsWindows || MelonLaunchOptions.Console.ShouldHide)
            return;
        
        ConsoleOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        ConsoleOutStream = new FileStream(new SafeFileHandle(ConsoleOutHandle, false), FileAccess.Write);
        ConsoleOutWriter = new StreamWriter(ConsoleOutStream);
        ConsoleOutWriter.AutoFlush = true;
    }

    internal static void WriteLine(string txt)
    {
        if (MelonUtils.IsUnderWineOrSteamProton() || !MelonUtils.IsWindows || MelonLaunchOptions.Console.ShouldHide)
        {
            Console.WriteLine(txt);
            return;
        }
        ConsoleOutWriter.WriteLine(txt);
    }

    internal static void WriteLine(object txt)
    {
        if (MelonUtils.IsUnderWineOrSteamProton() || !MelonUtils.IsWindows || MelonLaunchOptions.Console.ShouldHide)
        {
            Console.WriteLine(txt.ToString());
            return;
        }
        ConsoleOutWriter.WriteLine(txt.ToString());
    }

    internal static void WriteLine()
    {
        if (MelonUtils.IsUnderWineOrSteamProton() || !MelonUtils.IsWindows || MelonLaunchOptions.Console.ShouldHide)
        {
            Console.WriteLine();
            return;
        }
        ConsoleOutWriter.WriteLine("");
    }
    
    internal static void HideConsole()
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        ShowWindow(consoleWindowHandle, 0);
    }
    
    internal static void ShowConsole()
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        ShowWindow(consoleWindowHandle, 1);
    }
    
    internal static void ToggleConsole()
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        var visible = IsWindowVisible(consoleWindowHandle);
        ShowWindow(consoleWindowHandle, visible ? 0 : 1);
    }
    
    internal static void SetConsoleRect(int x, int y, int width, int height)
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        SetWindowPos(consoleWindowHandle, IntPtr.Zero, x, y, width, height, 0);
    }
    
    internal static void SetConsoleRect(CorePreferences.FConsoleRect rect)
    {
        IntPtr consoleWindowHandle = GetConsoleWindow();
        SetWindowPos(consoleWindowHandle, IntPtr.Zero, rect.X, rect.Y, rect.Width, rect.Height, 0);
    }

    internal static StatusWindow.RECT GetConsoleRect()
    {
        StatusWindow.GetWindowRect(GetConsoleWindow(), out var rect);
        return rect;
    }

    internal static void SaveConsoleRect()
    {
        var rect = GetConsoleRect();
        CorePreferences.ConsoleRect.Value = new(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();
        
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    
    [DllImport("user32.dll", SetLastError=true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags);
}