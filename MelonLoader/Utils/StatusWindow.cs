using System;
using System.Runtime.InteropServices;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace MelonLoader.Utils;

internal class StatusWindow
{
    private const string ClassName = "SFLoaderStatusWindowClass";
    private const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
    private const uint WS_POPUP = 0x80000000;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int COLOR_WINDOW = 5;
    private const int CW_USEDEFAULT = unchecked((int)0x80000000);
    private const int WM_DESTROY = 0x0002;
    private const uint WM_QUIT = 0x0012;
    private const uint WM_CLOSE = 0x0010;
    private const uint WM_PAINT = 0x000F;
    private const uint WM_NCPAINT = 0x0085;

    private const int PS_SOLID = 0;
    private const int NULL_BRUSH = 5;

    private const int ERROR = 0;

    private const uint DT_CENTER = 0x1;
    private const uint DT_VCENTER = 0x4;
    private const uint DT_SINGLELINE = 0x20;

    private const int LOGPIXELSX = 88;
    private const int LOGPIXELSY = 90;

    private const uint DT_CALCRECT = 0x400;

    public static IntPtr WindowHandle;
    private static string _statusText = "Loading...";

    public static int WindowHeight { get; set; }

    public static int WindowWidth { get; set; }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("gdi32.dll", EntryPoint = "CreateSolidBrush", SetLastError = true)]
    public static extern IntPtr CreateSolidBrush(uint color);

    [DllImport("user32.dll")]
    public static extern int DrawText(IntPtr hdc, string lpString, int nCount, ref RECT lpRect, uint uFormat);

    [DllImport("user32.dll")]
    public static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

    [DllImport("gdi32.dll")]
    public static extern bool SetTextColor(IntPtr hdc, uint crColor);

    [DllImport("gdi32.dll")]
    public static extern int SetBkMode(IntPtr hdc, int mode);

    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse,
        int nHeightEllipse);

    [DllImport("user32.dll")]
    public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateFont(int height, int width, int escapement, int orientation, int weight, uint italic, uint underline,
        uint strikeOut, uint charSet, uint outPrecision, uint clipPrecision, uint quality, uint pitchAndFamily, string faceName);

    [DllImport("user32.dll")]
    public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

    [DllImport("gdi32.dll")]
    public static extern IntPtr GetStockObject(int fnObject);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

    [DllImport("gdi32.dll")]
    public static extern bool FrameRgn(IntPtr hdc, IntPtr hrgn, IntPtr hbr, int nWidth, int nHeight);

    public static void Show()
    {
        var windowThread = new Thread(CreateWindow);
        windowThread.Start();

        Console.WriteLine("Window is running in another thread!");
    }

    public static void CloseWindow()
    {
        SendMessage(WindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("gdi32.dll")]
    public static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern bool RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    
    [DllImport("user32.dll")]
    private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

    private static RECT CalcTextRect(IntPtr hdc, string text, IntPtr font)
    {
        var tempRect = new RECT();
        var oldFont = SelectObject(hdc, font);
        DrawText(hdc, text, -1, ref tempRect, DT_CALCRECT);
        SelectObject(hdc, oldFont);
        return tempRect;
    }

    private static int ScaleForDpi(int value, int dpi)
    {
        return value * dpi / 96;
    }

    public static string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            if (WindowHandle != IntPtr.Zero)
            {
                InvalidateRect(WindowHandle, IntPtr.Zero, true);
            }
        }
    }

    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_PAINT:
            {
                PAINTSTRUCT ps;
                var hdc = BeginPaint(hWnd, out ps);

                var rect = new RECT();
                GetClientRect(hWnd, ref rect);
                var width = rect.Right - rect.Left;
                var height = rect.Bottom - rect.Top;

                // Draw red border
                var hPen = CreatePen(PS_SOLID, 10, 0x100763);
                var hOldPen = SelectObject(hdc, hPen);
                var hOldBrush = SelectObject(hdc, GetStockObject(NULL_BRUSH));

                var hRgn = CreateRoundRectRgn(0, 0, width, height, width / 2, height / 2);
                FrameRgn(hdc, hRgn, hPen, 2, 2);

                SelectObject(hdc, hOldBrush);
                SelectObject(hdc, hOldPen);
                DeleteObject(hPen);
                DeleteObject(hRgn);

                var dpiY = GetDeviceCaps(hdc, LOGPIXELSY);

                var bigFontSize = ScaleForDpi(32, dpiY);
                var regularFontSize = ScaleForDpi(14, dpiY);

                var bigFont = CreateFont(bigFontSize, 0, 0, 0, 700, 0, 0, 0, 0, 3, 0, 1, 0, "Arial");
                var font = CreateFont(regularFontSize, 0, 0, 0, 700, 0, 0, 0, 0, 3, 0, 1, 0, "Arial");

                var processingRect = CalcTextRect(hdc, "Processing", bigFont);
                var loadingRect = CalcTextRect(hdc, "Loading", font);
                
                var spacingBetweenTexts = 30;

                var combinedHeight = (processingRect.Bottom - processingRect.Top) + (loadingRect.Bottom - loadingRect.Top) + spacingBetweenTexts;

                var processingTop = (height - combinedHeight) / 2;
                var loadingTop = processingTop + (processingRect.Bottom - processingRect.Top) + spacingBetweenTexts;
                
                SetTextColor(hdc, 0x00777777);
                SetBkMode(hdc, 1);

                var bigTextRect = rect with { Top = processingTop, Bottom = processingTop + (processingRect.Bottom - processingRect.Top) };
                SelectObject(hdc, bigFont);
                DrawText(hdc, "SFLoader", -1, ref bigTextRect, DT_CENTER | DT_SINGLELINE);
                
                SetTextColor(hdc, 0x00FFFFFF);
                SetBkMode(hdc, 1);

                var smallTextRect = rect with { Top = loadingTop, Bottom = loadingTop + (loadingRect.Bottom - loadingRect.Top) };
                SelectObject(hdc, font);
                DrawText(hdc, StatusText, -1, ref smallTextRect, DT_CENTER | DT_SINGLELINE);

                DeleteObject(bigFont);
                DeleteObject(font);

                EndPaint(hWnd, ref ps);
            }
                return IntPtr.Zero;
            case WM_DESTROY:
                PostMessage(hWnd, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                return IntPtr.Zero;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static void CreateWindow()
    {
        var screenWidth = GetSystemMetrics(0);
        var screenHeight = GetSystemMetrics(1);

        var scaling = 1f;
        
        var hdc = GetDC(IntPtr.Zero);
        if (hdc != IntPtr.Zero)
        {
            var dpi = GetDeviceCaps(hdc, LOGPIXELSX); // LOGPIXELSX = 88
            ReleaseDC(IntPtr.Zero, hdc);

            scaling = dpi / 96.0f;
        }

             

        WindowWidth = (int)(350 * scaling);
        WindowHeight = WindowWidth;

        var x = (screenWidth - WindowWidth) / 2;
        var y = (screenHeight - WindowHeight) / 2;

        var hRgn = CreateRoundRectRgn(0, 0, WindowWidth, WindowHeight, WindowWidth / 2, WindowHeight / 2);

        var wc = new WNDCLASS
        {
            style = 0,
            lpfnWndProc = new WndProcDelegate(WndProc),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = GetModuleHandle(null),
            hIcon = IntPtr.Zero,
            hCursor = IntPtr.Zero,
            hbrBackground = CreateSolidBrush(0x00111111),
            lpszMenuName = null,
            lpszClassName = ClassName
        };

        if (RegisterClass(ref wc) == false)
        {
            MelonLogger.Error("Failed to register window class!");
            return;
        }

        WindowHandle = CreateWindowEx(
            WS_EX_TOPMOST | WS_EX_TOOLWINDOW, ClassName, "SFStatus",
            WS_POPUP, x, y, WindowWidth, WindowHeight,
            IntPtr.Zero, IntPtr.Zero, wc.hInstance, IntPtr.Zero);

        if (WindowHandle == IntPtr.Zero)
        {
            MelonLogger.Error("Couldn't create loader window!");
            return;
        }

        SetWindowRgn(WindowHandle, hRgn, true);

        ShowWindow(WindowHandle, 1);
        UpdateWindow(WindowHandle);

        while (GetMessage(out var msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }
    
    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct WNDCLASS
    {
        public uint style;
        public Delegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hWnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }
}