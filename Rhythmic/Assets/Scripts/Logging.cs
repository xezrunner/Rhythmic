using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RH_LogFunction_Signature = System.Action<string, Logging.LogLevel>;

public static class Logging {
    public enum LogTarget {
        Unity = 0,
        IngameConsole = 1 << 0, IngameQuickline = 1 << 1,
        ExternalConsole = 1 << 2,
        All = 0x11111111 // TODO: Look up how long this should be!
    }
    public enum LogLevel {
        Info = 0, Warning = 1, Error = 2, // Unity supports these only.
        Gameplay = 3, IO = 4, Streaming = 5,
        // ...
    }
    
    public record Logging_Options {
        public LogLevel  default_level   = LogLevel.Info;
        public LogTarget default_targets = LogTarget.All;

        public CallerDebugInfoFlags caller_info = CallerDebugInfoFlags.FP;
    }
    public static Logging_Options options = new();

    public static bool log(string message, LogTarget targets, LogLevel level, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) {
        message = message.add_caller_debug_info(options.caller_info, caller_file_path, caller_proc_name, caller_line_num);
        log_to_targets(message, targets, level);
        return true;
    }
    public static bool log(object message_obj, LogTarget targets, LogLevel level, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) {
        return log(message_obj.ToString(), targets, level, caller_file_path, caller_proc_name, caller_line_num);
    }

    #region Overloads
    // No extra args:
    public static bool log(object message_obj, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) => log(message_obj, options.default_targets, options.default_level,                                      caller_file_path, caller_proc_name, caller_line_num);
    public static bool log(string message, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) => log(message, options.default_targets, options.default_level, 
                                                                            caller_file_path, caller_proc_name, caller_line_num);

    public static bool log_warn(object message_obj,
    [CallerFilePath]   string caller_file_path = null,
    [CallerMemberName] string caller_proc_name = null,
    [CallerLineNumber] int caller_line_num = -1) => log(message_obj, options.default_targets, LogLevel.Warning,
                                                        caller_file_path, caller_proc_name, caller_line_num);
    public static bool log_warn(string message,
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) => log(message, options.default_targets, LogLevel.Warning,
                                                                            caller_file_path, caller_proc_name, caller_line_num);

    public static bool log_error(object message_obj,
    [CallerFilePath]   string caller_file_path = null,
    [CallerMemberName] string caller_proc_name = null,
    [CallerLineNumber] int caller_line_num = -1) => log(message_obj, options.default_targets, LogLevel.Error,
                                                        caller_file_path, caller_proc_name, caller_line_num);
    public static bool log_error(string message,
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) => log(message, options.default_targets, LogLevel.Error,
                                                                            caller_file_path, caller_proc_name, caller_line_num);

    // LogLevel arg:
    public static bool log(object message_obj, LogLevel level, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) =>
    log(message_obj, options.default_targets, level, 
                          caller_file_path, caller_proc_name, caller_line_num);
    public static bool log(string message, LogLevel level, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) =>
        log(message, options.default_targets, level, 
                          caller_file_path, caller_proc_name, caller_line_num);
    #endregion

    static void log_to_targets(string s, LogTarget targets, LogLevel level) {
        if (targets.HasFlag(LogTarget.Unity)) UNITY_Log(s, level);

        List<RH_LogFunction_Signature> functions_to_call = RH_GetLogFunctions(targets, level);
        foreach (var func in functions_to_call) func.Invoke(s, level);
    }

    #region Rhythmic
    static List<RH_LogFunction_Signature> RH_GetLogFunctions(LogTarget targets, LogLevel level) {
        List<RH_LogFunction_Signature> list = new();
        if (targets.HasFlag(LogTarget.IngameConsole)) list.Add(DebugConsole.write_line);

        return list;
    }
    static bool RH_IngameConsole_Log(string s, LogLevel level) {
        return true;
    }
    #endregion

    #region Unity
    static Action<object> UNITY_GetLogLevelFunction(LogLevel level) {
        switch (level) {
            default:
            case LogLevel.Info: return UnityEngine.Debug.Log;
            case LogLevel.Warning: return UnityEngine.Debug.LogWarning;
            case LogLevel.Error: return UnityEngine.Debug.LogError;
        }
    }
    static bool UNITY_Log(string s, LogLevel level) {
        Action<object> action_to_call = UNITY_GetLogLevelFunction(level);
        action_to_call.Invoke(s);
        return true;
    }
    #endregion
}