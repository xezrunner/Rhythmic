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

public enum LogTarget
{
    Unity = 0, RhythmicConsole = 1 << 0, DebugLine = 1 << 1,
    Default = (Unity | RhythmicConsole),
    All = (Unity | RhythmicConsole | DebugLine)
}

public class Logger
{
    public static LogTarget CurrentLogTarget = LogTarget.All;

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

    // TODO: support colors for 'object' too?
    public static void LogUnity(object obj, CLogType logType) => GetUnityLogHandlerForLogType(logType)(obj);
    public static void LogUnity(object obj) => Debug.Log(obj);
    public static void LogUnity(string text, CLogType logType) => GetUnityLogHandlerForLogType(logType)(text.AddColor(Colors.GetColorForCLogType(logType)));
    public static void LogUnity(string text) => Debug.Log(text);
    public static string LogConsole(string text, CLogType logType) { if (ConsoleServer.IsServerActive) ConsoleServer.Write(text, logType); return text; }

    public static string Log(string text, CLogType logType, LogTarget logTarget = LogTarget.All)
    {
        if (logTarget.HasFlag(LogTarget.Unity) && CurrentLogTarget.HasFlag(LogTarget.Unity)) LogUnity(text, logType);
        if (logTarget.HasFlag(LogTarget.RhythmicConsole) && CurrentLogTarget.HasFlag(LogTarget.RhythmicConsole)) LogConsole(text, logType);
        if (logTarget.HasFlag(LogTarget.DebugLine) && CurrentLogTarget.HasFlag(LogTarget.DebugLine)) DebugUI.AddToDebugLine(text, Colors.GetColorForCLogType(logType)); // TODO: methods for this?

        return text;
    }
    // Log without logging to Unity
    static string LogR(string text, CLogType logType = CLogType.Info) => Log(text, logType, CurrentLogTarget & ~LogTarget.Unity);

    /// <summary>
    /// This method handles logging out debugging information for supported Types.
    /// </summary>
    /// <param name="obj">The object to log out debug information from.</param>
    public static string Log(object obj, CLogType logType = 0, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All)
    {
        switch (obj)
        {
            default:
                LogUnity(obj);
                return LogR($"Unsupported object passed to Logger | Name: {nameof(obj)}, Type: {obj.GetType()}");

            case string s: return Log(s, logType, logTarget);
            case Array a: return LogArray(a, logType, printIndex, separatorChar, logTarget);
            case List<string> l: return LogList(l, logType, printIndex, separatorChar, logTarget);
            case List<int> l: return LogList(l, logType, printIndex, separatorChar, logTarget);
            case List<float> l: return LogList(l, logType, printIndex, separatorChar, logTarget);
        }
    }

    /// <summary>Simple text logging.</summary>
    public static string Log(string text) => Log(text, CLogType.Info);

    /// <param name="objToType">Pass in 'this' to print out origin class name before the text.<br/>
    /// You can also pass in a string if you want custom text before the log text.</param>
    /// <param name="printMethodName">Whether to show the calling method (function) name.</param>
    public static string Log(string text, object objToType, bool printMethodName = false, LogTarget logTarget = LogTarget.All, CLogType logType = 0, [CallerMemberName] string methodName = null)
    {
        string cName = "";
        if (objToType != null)
            if (objToType.GetType() == typeof(string)) cName = (string)objToType; // Automatically use the string value, in case you want custom text
            else cName = objToType.GetType().Name;

        string mName = printMethodName && (methodName != null && methodName != "") ? ((cName != "") ? $"/{methodName}()" : "") : ""; // .../methodName(): <text> | ignores '/' when there's no class name

        return Log($"{cName}{mName}: {text}", logType, logTarget); // Type/Method(): text
    }

    /// <summary>Logs the method name before the desired text.</summary>
    /// <param name="objToType">Pass in 'this' to print out origin class name before the text.</param>
    public static string LogMethod(string text, object objToType = null, LogTarget logTarget = LogTarget.All, CLogType logType = 0, [CallerMemberName] string methodName = null) => Log(text, objToType, true, logTarget, logType, methodName);

    public static string LogObject(object obj, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => Log(obj, 0, printIndex, separatorChar, logTarget);
    public static string LogFormat(string text, CLogType logType, LogTarget logTarget = LogTarget.All, params object[] args) => Log(string.Format(text, args), logType, logTarget);
    public static string LogFormat(string text, LogTarget logTarget = LogTarget.All, params object[] args) => Log(string.Format(text, args), logTarget);

    public static string LogWarning(string text, LogTarget logTarget = LogTarget.All) => Log(text, CLogType.Warning, logTarget);
    public static string LogObjectWarning(object obj, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => Log(obj, CLogType.Warning, printIndex, separatorChar, logTarget);

    public static string LogError(string text, LogTarget logTarget = LogTarget.All) => Log(text, CLogType.Error, logTarget);
    public static string LogObjectError(object obj, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => Log(obj, CLogType.Error, printIndex, separatorChar, logTarget);

    /// Log() for special/non-string types

    // Arrays:
    public static string LogArray(Array array, CLogType logType, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All)
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
        return Log(s, logType, logTarget); // Log!
    }
    // Lists:
    // redirected to array logging:
    public static string LogList<T>(List<T> list, CLogType logType, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => LogArray(list.ToArray(), logType, printIndex, separatorChar, logTarget);
}