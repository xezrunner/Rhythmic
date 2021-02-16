using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    public static void LogConsole(string text, CLogType logType) { if (ConsoleServer.IsServerActive) ConsoleServer.Write(text, logType); }

    public static void Log(string text, CLogType logType)
    {
        LogUnity(text, logType);
        LogConsole(text, logType);
        //if (DebugUI.Instance && logType > CLogType.Unimportant) DebugUI.AddToDebugLine(text); // TEMP!
    }

    /// <summary>Simple text logging.</summary>
    public static void Log(string text) => Log(text, CLogType.Info);

    /// <param name="objToType">Pass in 'this' to print out origin class name before the text.<br/>
    /// You can also pass in a string if you want custom text before the log text.</param>
    /// <param name="printMethodName">Whether to show the calling method (function) name.</param>
    public static void Log(string text, object objToType, bool printMethodName = false, [CallerMemberName] string methodName = null)
    {
        string cName = "";
        if (objToType != null)
            if (objToType.GetType() == typeof(string)) cName = (string)objToType; // Automatically use the string value, in case you want custom text
            else cName = objToType.GetType().Name;

        string mName = printMethodName && (methodName != null && methodName != "") ? ((cName != "") ? $"/{methodName}()" : "") : ""; // .../methodName(): <text> | ignores '/' when there's no class name

        Log($"{cName}{mName}: {text}", CLogType.Application); // Type/Method(): text
    }

    /// <summary>Logs the method name before the desired text.</summary>
    /// <param name="objToType">Pass in 'this' to print out origin class name before the text.</param>
    public static void LogMethod(string text, object objToType = null, [CallerMemberName] string methodName = null) => Log(text, objToType, true, methodName);

    public static void LogObject(object obj, bool printIndex = true, char separatorChar = ',') => Log(obj, 0, printIndex, separatorChar);
    public static void LogFormat(string text, CLogType logType, params object[] args) => Log(string.Format(text, args), logType);
    public static void LogFormat(string text, params object[] args) => Log(string.Format(text, args));

    public static void LogWarning(string text) => Log(text, CLogType.Warning);
    public static void LogObjectWarning(object obj, bool printIndex = true, char separatorChar = ',') => Log(obj, CLogType.Warning, printIndex, separatorChar);

    public static void LogError(string text) => Log(text, CLogType.Error);
    public static void LogObjectError(object obj, bool printIndex = true, char separatorChar = ',') => Log(obj, CLogType.Error, printIndex, separatorChar);

    /// Log() for special/non-string types

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