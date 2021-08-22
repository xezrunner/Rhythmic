using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum CLogType { None = -1, Info = 0, Unimportant = 1, Warning = 2, Error = 3, Caution = 4, Network = 5, IO = 6, Application = 7, UNKNOWN = 99 }
public enum LogLevel { None, Warning, Error }
[Flags]
public enum LogTarget { Unity = 1 << 0, DebugUI = 1 << 1, All = (Unity | DebugUI) }

public static class Logger
{
    // TODO: Re-add DebugUI compatibility!

    // Main log:
    public static void Log(object text, LogTarget target, LogLevel level, params object[] args) => LoggerActionCalls.ExecuteLogActions(target, level, text.ToString().Parse(args));

    // Log Overloads:
    public static void Log(object text, LogLevel level, params object[] args) => Log(text, LogTarget.All, level, args);
    public static void Log(object text, params object[] args) => Log(text, LogTarget.All, LogLevel.None, args);

    // Warning and Error overloads:
    public static void LogWarning(object text, params object[] args) => Log(text, LogLevel.Warning, args);
    public static void LogW(object text, params object[] args) => LogWarning(text, args);
    public static void LogError(object text, params object[] args) => Log(text, LogLevel.Error, args);
    public static void LogE(object text, params object[] args) => LogError(text, args);
}
