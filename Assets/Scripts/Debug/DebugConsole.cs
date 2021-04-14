using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public static class DebugConsole
{
    [DllImport("Kernel32.dll")]
    private static extern bool AttachConsole(uint processId);

    [DllImport("Kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("Kernel32.dll")]
    private static extern bool FreeConsole();

    [DllImport("Kernel32.dll")]
    private static extern bool SetConsoleTitle(string title);

    [DllImport("Kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    static TextWriter prev_output;
    static string current_line;

    public static void Init()
    {
        if (!AttachConsole(0xffffffff))
            AllocConsole();

        prev_output = Console.Out;
        SetConsoleTitle("Rhythmic console");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear();
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        current_line = "";
    }

    public static void Shutdown()
    {
        FreeConsole();
    }
}