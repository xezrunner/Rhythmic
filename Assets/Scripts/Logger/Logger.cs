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
    static Action<object> GetUnityLogHandlerForLogType(CLogType logType)
    {
        switch (logType)
        {
            default:
                return Debug.Log;

            case CLogType.Warning:
                return Debug.LogWarning;
            case CLogType.Error:
            case CLogType.Caution:
                return Debug.LogError;
        }
    }
    public static void LogUnity(object obj, CLogType logType) => GetUnityLogHandlerForLogType(logType)(obj);
    public static void LogUnity(object obj) => Debug.Log(obj);

    /// <summary>
    /// This method handles logging out debugging information for supported Types.
    /// </summary>
    /// <param name="obj">The object to log out debug information from.</param>
    public static void Log(object obj, CLogType logType = 0, bool printIndex = true, char separatorChar = ',')
    {
        switch (obj)
        {
            default:
                //Log($"Unsupported object passed to Logger | Type: {obj.GetType()}"); break;
                LogUnity(obj); break;

            case string s: Log(s, logType); break;
            case Array a: LogArray(a, logType, printIndex, separatorChar); break;
            case List<string> l: LogList(l, logType, printIndex, separatorChar); break;
            case List<int> l: LogList(l, logType, printIndex, separatorChar); break;
            case List<float> l: LogList(l, logType, printIndex, separatorChar); break;
        }
    }

    public static void LogConsole(string text, CLogType logType)
    {
        if (ConsoleServer.IsServerActive) ConsoleServer.Write(text, logType);
    }

    public static void Log(string text, CLogType logType)
    {
        LogUnity(text, logType);
        LogConsole(text, logType);
    }
    public static void Log(string text) => Log(text, CLogType.Info);
    public static void Log(object obj, bool printIndex = true, char separatorChar = ',') => Log(obj, 0, printIndex, separatorChar);
    public static void LogFormat(string text, CLogType logType, params object[] args) => Log(string.Format(text, args), logType);
    public static void LogFormat(string text, params object[] args) => Log(string.Format(text, args));

    public static void LogWarning(object obj, bool printIndex = true, char separatorChar = ',') => Log(obj, CLogType.Warning, printIndex, separatorChar);
    public static void LogWarning(string text) => Log(text, CLogType.Warning);
    public static void LogError(object obj, bool printIndex = true, char separatorChar = ',') => Log(obj, CLogType.Error, printIndex, separatorChar);
    public static void LogError(string text) => Log(text, CLogType.Error);

    /// Log() for types

    // Arrays:
    public static void LogArray(Array array, CLogType logType, bool printIndex = true, char separatorChar = ',')
    {
        string s = "";

        int i = 0;
        foreach (var o in array) // Add element values to the string
        {
            // printIndex: true | [0]: first, [1]: second, [2]: third
            // printIndex: false | first, second, third
            s += (!printIndex ? "" : $"[{i}]: ") + o.ToString() + $"{separatorChar} ";
            i++;
        }

        s = s.Substring(0, s.Length - 2); // Remove trailing separator
        Log(s, logType); // Log!
    }
	// Lists:
	// redirected to array logging:
    public static void LogList<T>(List<T> list, CLogType logType, bool printIndex = true, char separatorChar = ',') => LogArray(list.ToArray(), logType, printIndex, separatorChar);
}