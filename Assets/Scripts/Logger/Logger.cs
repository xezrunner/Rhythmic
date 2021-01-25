using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logger:
/// Logger is an abstraction for logging debugging information to both the Unity Console and the 
/// standalone debug console app. <br/>
/// It is possible to pass non-string types, which may be handled to print out useful information.
/// <br/>
/// See <see cref="ConsoleServer"/> for the enums used here. (<see cref="CLogType"/>)
/// </summary>

public class Logger
{
    /// <summary>
    /// This method handles logging out debugging information for supported Types.
    /// </summary>
    /// <param name="obj">The object to log out debug information from.</param>
    public static void Log(object obj, CLogType logType = 0, bool printIndex = true, char separatorChar = ',')
    {
        switch (obj)
        {
            default:
                Log($"Unsupported object passed to Logger | Type: {obj.GetType()}"); break;

            case string s: Log(s, logType); break;
            case List<string> l: LogList(l, logType, printIndex, separatorChar); break;
            case List<int> l: LogList(l, logType, printIndex, separatorChar); break;
            case List<float> l: LogList(l, logType, printIndex, separatorChar); break;
        }
    }

    public static void Log(string text, CLogType logType = 0)
    {
        Debug.Log(text);
        if (ConsoleServer.IsServerActive) ConsoleServer.Write(text, logType);
    }
    public static void LogFormat(string text, params object[] args) => Log(string.Format(text, args));

    /// Log() for types

    // Lists:
    public static void LogList<T>(List<T> list, CLogType logType, bool printIndex = true, char separatorChar = ',')
    {
        string s = "";

        int i = 0;
        foreach (var o in list) // Add element values to the string
        {
            // printIndex: true | [0]: first, [1]: second, [2]: third
            // printIndex: false | first, second, third
            s += (!printIndex ? "" : $"[{i}]: ") + o.ToString() + $"{separatorChar} ";
            i++;
        }

        s = s.Substring(0, s.Length - 2); // Remove trailing separator
        Log(s, logType); // Log!
    }
}