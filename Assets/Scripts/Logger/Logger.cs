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
    UnityAndConsole = (Unity | RhythmicConsole),
    Default = UnityAndConsole, All = (Unity | RhythmicConsole | DebugLine)
}

// TODO: Add LogWarning & LogError (& LogIO, LogNetwork, LogApplication, LogGame)(?) variatons!
public static partial class Logger
{
    public static LogTarget CurrentLogTarget = LogTarget.All;

    /// Unity logging:
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
    public static void LogUnity(object obj) => Debug.Log(obj);
    public static void LogUnity(object obj, CLogType logType) => GetUnityLogHandlerForLogType(logType)(obj);

    /// RhythmicConsole logging:
    public static string LogConsole(string text, CLogType logType) { if (ConsoleServer.IsServerActive) ConsoleServer.Write(text, logType); return text; }

    /// Log():

    // Logging router:
    public static string Log(string text, CLogType logType, LogTarget logTarget = LogTarget.All)
    {
        if (logTarget.HasFlag(LogTarget.Unity) && CurrentLogTarget.HasFlag(LogTarget.Unity)) LogUnity(text, logType);
        if (logTarget.HasFlag(LogTarget.RhythmicConsole) && CurrentLogTarget.HasFlag(LogTarget.RhythmicConsole)) LogConsole(text, logType);
        if (Application.isPlaying)
            if (logTarget.HasFlag(LogTarget.DebugLine) && CurrentLogTarget.HasFlag(LogTarget.DebugLine)) DebugUI.AddToDebugLine(text, Colors.GetColorForCLogType(logType)); // TODO: methods for this?

        return text;
    }
    // Log without logging to Unity
    static string LogR(string text, CLogType logType = CLogType.Info) => Log(text, logType, CurrentLogTarget & ~LogTarget.Unity);

    // Object logging (handling objects):
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

            // TODO: Vector2,3,4
            case string s: return Log(s, logType, logTarget);
            case Array a: return LogArray(a, logType, printIndex, separatorChar, logTarget);
            case List<string> l: return LogList(l, logType, printIndex, separatorChar, logTarget);
            case List<int> l: return LogList(l, logType, printIndex, separatorChar, logTarget);
            case List<float> l: return LogList(l, logType, printIndex, separatorChar, logTarget);
        }
    }
    // Simple text logging:
    public static string Log(string text) => Log(text, CLogType.Info);
    // Class/Method() logging:
    /// <param name="objToType">Pass in 'this' to print out origin class name before the text.<br/>
    /// You can also pass in a string if you want custom text before the log text.</param>
    /// <param name="printMethodName">Whether to show the calling method (function) name.</param>
    // TODO: do we need printMethodName? We can just pass in "" or null instead.
    public static string Log(string text, object objToType, bool printMethodName = false, CLogType logType = 0, LogTarget logTarget = LogTarget.All, [CallerMemberName] string methodName = null)
    {
        // Build class name:
        string cName = ""; // className
        if (objToType != null)
            if (objToType.GetType() == typeof(string)) cName = (string)objToType; // Automatically use the string value, in case you want custom text
            else cName = objToType.GetType().Name;

        // Build method name:
        string mName = ""; // .../methodName()
        if (printMethodName && (methodName != null || methodName != ""))
        {
            mName += cName != "" ? "/" : ""; // '/' character after cName, if exists
            mName += $"{methodName}()";
        }

        return Log($"{cName}{mName}: {text}", logType, logTarget); // Type/Method(): text
    }

    /// -------

    // Special/non-string type logging:
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
    // Lists - redirected to array logging:
    public static string LogList<T>(List<T> list, CLogType logType, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => LogArray(list.ToArray(), logType, printIndex, separatorChar, logTarget);
}