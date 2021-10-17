// These define which action calls you want to be able to execute. Undefined entries won't be called.
#define UNITY
#define DebugUI
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

#if DebugUI
        // tba...
#endif

#if DebugConsole
        if (target.HasFlag(LogTarget.DebugConsole))
        {
            if (level > LogLevel.None)
                DebugConsole.ConsoleLog(text.AddColor(GetLogLevelColor(level)));
            else
                DebugConsole.ConsoleLog(text);
        }
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