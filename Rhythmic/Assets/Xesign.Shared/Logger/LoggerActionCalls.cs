// These define which action calls you want to be able to execute. Undefined entries won't be called.
#define UNITY
#define DebugQuickLine
#define DebugConsole

using System;
using UnityEngine;

public static class LoggerActionCalls
{
    public static void ExecuteLogActions(LogTarget target, LogLevel level, string text)
    {
#if UNITY
        if (target.HasFlag(LogTarget.Unity)) UNITY_GetLogLevelAction(level).Invoke(text);
#endif

        if (level > LogLevel.None)
            text = text.AddColor(GetLogLevelColor(level));

#if DebugQuickLine
        if (target.HasFlag(LogTarget.DebugQuickLine))
            DebugSystem.QuickLineLog(text);
#endif

#if DebugConsole
        if (target.HasFlag(LogTarget.DebugConsole))
            DebugConsole.ConsoleLog(text);
#endif
    }

    // Action call gathering:

#if UNITY
    static Action<object> UNITY_GetLogLevelAction(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.None: return Debug.Log;
            case LogLevel.Warning: return Debug.LogWarning;
            case LogLevel.Error: return Debug.LogError;
            default: return null;
        }
    }
#endif

    // Colors for DebugConsole:
    static Color GetLogLevelColor(LogLevel level)
    {
        switch (level)
        {
            default: return Colors.Info;
            case LogLevel.Warning: return Colors.Warning;
            case LogLevel.Error: return Colors.Error;
        }
    }
}