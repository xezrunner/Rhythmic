using System.Runtime.CompilerServices;
using UnityEngine;

public partial class Logger
{
    /// Unity logging:
    public static void LogUnity(string text) => Debug.Log(text);
    public static void LogUnity(string text, CLogType logType) => GetUnityLogHandlerForLogType(logType)(text.AddColor(Colors.GetColorForCLogType(logType)));

    /// Standard warn/error:
    public static string LogWarning(string text, LogTarget logTarget = LogTarget.All, params string[] args) => Log(text, CLogType.Warning, logTarget, args);
    public static string LogWarning(string text, params string[] args) => Log(text, CLogType.Warning, args);
    public static string LogW(string text, params string[] args) => LogWarning(text, args);

    public static string LogError(string text, LogTarget logTarget = LogTarget.All, params string[] args) => Log(text, CLogType.Error, logTarget, args);
    public static string LogError(string text, params string[] args) => Log(text, CLogType.Error, args);
    public static string LogE(string text, params string[] args) => LogError(text, args);

    /// Object logging:
    public static string LogObject(object obj, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => Log(obj, 0, printIndex, separatorChar, logTarget);
    public static string LogObjectWarning(object obj, bool printIndex = true, char separatorChar = ',', LogTarget logTarget = LogTarget.All) => Log(obj, CLogType.Warning, printIndex, separatorChar, logTarget);

    /// String format: -- shouldn't use! --
#if false // Deprecated
    static string LogFormat(string text, CLogType logType, LogTarget logTarget, params object[] args) => Log(string.Format(text, args), logType, logTarget);
    static string LogFormat(string text, CLogType logType, params object[] args) => Log(string.Format(text, args), logType, CurrentLogTarget);
    static string LogFormat(string text, params object[] args) => Log(string.Format(text, args), logTarget: CurrentLogTarget);
    // Warning:
    static string LogWarningFormat(string text, params object[] args) => LogFormat(text, CLogType.Warning, args);
    static string LogWarningFormat(string text, LogTarget logTarget, params object[] args) => LogFormat(text, CLogType.Warning, logTarget, args);
    static string LogFormatW(string text, params object[] args) => LogWarningFormat(text, args);
    // Error:
    static string LogErrorFormat(string text, params object[] args) => LogFormat(text, CLogType.Error, args);
    static string LogErrorFormat(string text, LogTarget logTarget, params object[] args) => LogFormat(text, CLogType.Error, logTarget, args);
    static string LogFormatE(string text, params object[] args) => LogErrorFormat(text, args);
#endif

    /// LogMethod():
    // Standard:
    /// <summary>Logs the method name before the desired text.</summary>
    /// <param name="objToType">Pass in 'this' to print out origin class name before the text.</param>
    // TODO: Warning: When we want to specify LogTarget, it will see it as an object and call the method with objToType param instead!
    // That is very erroneous and wrong!!!
    /// DEPRECATED: [CallerMemberName] and params args don't go together, unfortunately.
    public static string LogMethod(string text, CLogType logType, LogTarget logTarget, object objToType = null, [CallerMemberName] string methodName = null) => _Log_Method(text, objToType, true, logType, logTarget, methodName);
    public static string LogMethod(string text, CLogType logType, object objToType = null, [CallerMemberName] string methodName = "") => _Log_Method(text, objToType, true, logType, CurrentLogTarget, methodName);
    public static string LogMethod(string text = "", object objToType = null, [CallerMemberName] string methodName = "") => _Log_Method(text, objToType, true, CLogType.Info, CurrentLogTarget, methodName);
    // TODO: is logTarget even a neccessary param in these? You could just call Log() with the method params that has LogTarget anyway...
    // Warning:
    public static string LogMethodW(string text = "", object objToType = null, [CallerMemberName] string methodName = null, LogTarget logTarget = LogTarget.All) => LogMethod(text, CLogType.Warning, logTarget, objToType, methodName);
    //public static string LogMethodW(string text = "", object objToType = null, [CallerMemberName] string methodName = null) => LogMethod(text, CLogType.Warning, CurrentLogTarget, objToType, methodName);
    // Error:
    public static string LogMethodE(string text = "", object objToType = null, [CallerMemberName] string methodName = null, LogTarget logTarget = LogTarget.All) => LogMethod(text, CLogType.Error, logTarget, objToType, methodName);
    //public static string LogMethodE(string text = "", object objToType = null, [CallerMemberName] string methodName = null) => LogMethod(text, CLogType.Error, CurrentLogTarget, objToType, methodName);
}