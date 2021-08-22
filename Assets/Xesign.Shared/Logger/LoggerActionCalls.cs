// These define which action calls you want to be able to execute. Undefined entries won't be called.
#define UNITY
#define DebugUI

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
}