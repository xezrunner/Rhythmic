using System.Runtime.CompilerServices;
using UnityEngine;

public partial class Logger
{
    /// Unity logging:
    public static void LogUnity(string text) => Debug.Log(text);
    public static void LogUnity(string text, CLogType logType) => GetUnityLogHandlerForLogType(logType)(text.AddColor(Colors.GetColorForCLogType(logType)));

    /// Standard warn/error:
    public static string LogWarning(string text, LogTarget logTarget = LogTarget.All) => Log(text, CLogType.Warning, logTarget);
    public static string LogError(string text, LogTarget logTarget = LogTarget.All) => Log(text, CLogType.Error, logTarget);
    public static string LogW(string text, LogTarget logTarget = LogTarget.All) => LogWarning(text, logTarget);
    public static string LogE(string text, LogTarget logTarget = LogTarget.All) => LogError(text, logTarget);

    /// Object logging:
    public static string LogObject(object obj, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => Log(obj, 0, printIndex, separatorChar, logTarget);
    public static string LogObjectWarning(object obj, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => Log(obj, CLogType.Warning, printIndex, separatorChar, logTarget);

    /// String format:
    public static string LogFormat(string text, CLogType logType, LogTarget logTarget, params object[] args) => Log(string.Format(text, args), logType, logTarget);
    public static string LogFormat(string text, CLogType logType, params object[] args) => Log(string.Format(text, args), logType, CurrentLogTarget);
    public static string LogFormat(string text, params object[] args) => Log(string.Format(text, args), logTarget: CurrentLogTarget);
    // Warning:
    public static string LogWarningFormat(string text, params object[] args) => LogFormat(text, CLogType.Warning, args);
    public static string LogWarningFormat(string text, LogTarget logTarget, params object[] args) => LogFormat(text, CLogType.Warning, logTarget, args);
    public static string LogFormatW(string text, params object[] args) => LogWarningFormat(text, args);
    // Error:
    public static string LogErrorFormat(string text, params object[] args) => LogFormat(text, CLogType.Error, args);
    public static string LogErrorFormat(string text, LogTarget logTarget, params object[] args) => LogFormat(text, CLogType.Error, logTarget, args);
    public static string LogFormatE(string text, params object[] args) => LogErrorFormat(text, args);

    /// LogMethod():
    // Standard:
    /// <summary>Logs the method name before the desired text.</summary>
    /// <param name="objToType">Pass in 'this' to print out origin class name before the text.</param>
    public static string LogMethod(string text = "", CLogType logType = 0, LogTarget logTarget = LogTarget.All, object objToType = null, [CallerMemberName] string methodName = null) => Log(text, objToType, true, logType, logTarget, methodName);
    public static string LogMethod(string text = "", CLogType logType = 0, object objToType = null, [CallerMemberName] string methodName = "") => Log(text, objToType, true, logType, CurrentLogTarget, methodName);
    public static string LogMethod(string text = "", object objToType = null, [CallerMemberName] string methodName = "") => Log(text, objToType, true, CLogType.Info, CurrentLogTarget, methodName);
    // TODO: is logTarget even a neccessary param in these? You could just call Log() with the method params that has LogTarget anyway...
    // Warning:
    public static string LogMethodW(string text = "", object objToType = null, [CallerMemberName] string methodName = null, LogTarget logTarget = LogTarget.All) => LogMethod(text, CLogType.Warning, logTarget, objToType, methodName);
    //public static string LogMethodW(string text = "", object objToType = null, [CallerMemberName] string methodName = null) => LogMethod(text, CLogType.Warning, CurrentLogTarget, objToType, methodName);
    // Error:
    public static string LogMethodE(string text = "", object objToType = null, [CallerMemberName] string methodName = null, LogTarget logTarget = LogTarget.All) => LogMethod(text, CLogType.Error, logTarget, objToType, methodName);
    //public static string LogMethodE(string text = "", object objToType = null, [CallerMemberName] string methodName = null) => LogMethod(text, CLogType.Error, CurrentLogTarget, objToType, methodName);
}