using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using XZShared;

// TODO:
// - Provide different log_ variants where caller debug info is disabled, log_console and such...

using XZ_LogFunction_Signature = System.Func<string, Logging.Logging_Info, bool>;

public static class Logging {
    public enum LogTarget {
        None = 0,
        Unity = 1 << 0,
        IngameConsole = 1 << 1, IngameQuickline = 1 << 2,
        ExternalConsole = 1 << 3,
        All = Unity | XZConsoles,
        XZConsoles = IngameConsole | IngameQuickline | ExternalConsole
    }
    [Flags] public enum LogLevel {
        None = 0,
        Info = 1 << 0, Warning = 1 << 1, Error = 1 << 2, // Unity supports these only.
        Gameplay = 1 << 3, IO = 1 << 4, Streaming = 1 << 5,
        Debug = 1 << 6,
        _ConsoleInternal = 1 << 30, _IgnoreFiltering = 1 << 31
        // ...
    }
    
    public struct Logging_Options {
        public LogLevel  default_level  ;
        public LogTarget default_targets;

        public CallerDebugInfoFlags caller_format_flags;
    }
    public static Logging_Options options = new() {
        default_level = LogLevel.Info,
        default_targets = LogTarget.All,
        caller_format_flags = CallerDebugInfoFlags.FP
    };

    public struct Logging_Info {
        public Logging_Info(string text, LogTarget target, LogLevel level, CallerDebugInfo caller_info = default) {
            this.text = text;
            this.target = target;
            this.level = level;
            this.caller_info = caller_info;
        }
        public string    text;
        public LogTarget target;
        public LogLevel  level;
        public CallerDebugInfo caller_info;
    }
    public static Logging_Info default_logging_info = new() {
        target = options.default_targets,
        level = options.default_level
    };

    // string:
    public static Logging_Info log(string message, LogTarget targets, LogLevel level, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int    caller_line_num = -1) {
        //message = message.add_caller_debug_info(options.caller_info, caller_file_path, caller_proc_name, caller_line_num);
        CallerDebugInfo caller_info = new CallerDebugInfo(caller_file_path, caller_proc_name, caller_line_num);
        return log_to_targets(message, targets, level, caller_info);
    }

    public static Logging_Info log_nocaller(string message, LogTarget targets, LogLevel level)     => log_to_targets(message, targets, level);
    public static Logging_Info log_nocaller(object message_obj, LogTarget targets, LogLevel level) => log(message_obj.ToString(), targets, level);

    static Logging_Info log_to_targets(string s, LogTarget targets, LogLevel level, CallerDebugInfo caller_info = default) {
        Logging_Info info = new(s, targets, level, caller_info);

        if (targets.HasFlag(LogTarget.Unity) || !DebugConsole.get_instance()) {
            if (DebugConsole.CONSOLE_RedirectUnityLogging && 
                level != LogLevel.Info && level != LogLevel.Warning && level != LogLevel.Error) {
                // This is not an Unity-supported log level. Let's temporarily disable the console log redirection,
                // let Unity log it with a default log level and meanwhile log it properly in our console:
                DebugConsole.CONSOLE_RedirectUnityLogging = false;
                UNITY_Log(s.add_caller_debug_info(options.caller_format_flags, caller_info), LogLevel.Info);
                DebugConsole.CONSOLE_RedirectUnityLogging = true;
            } else {
                UNITY_Log(s.add_caller_debug_info(options.caller_format_flags, caller_info), level);
                // If we are redirecting Unity's logging to the console, there's no need to handle this log message
                // ourselves - the redirection takes care of that:
                if (DebugConsole.CONSOLE_RedirectUnityLogging) return info;
            }
        }

        List<XZ_LogFunction_Signature> functions_to_call = XZ_GetLogFunctions(targets);
        foreach (var func in functions_to_call) func.Invoke(s, info);

        return info;
    }

    static Logging_Info log_dump_obj_internal(object obj, string name,
                                   [CallerFilePath]   string caller_file_path = null,
                                   [CallerMemberName] string caller_proc_name = null,
                                   [CallerLineNumber] int    caller_line_num = -1) {
        if (obj == null)
            log_error("null object!");

        Type obj_type = obj.GetType();

        BindingFlags flags           = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        FieldInfo[]  field_info      = obj_type.GetFields(flags);
        int          name_length_pad = field_info.Length > 0 ? field_info.Max(i => i.Name.Length) : 0;

        StringBuilder sb = new();
        sb.Append("\ndumping % fields for object of ".interp(field_info.Length));
        if (!name.is_empty()) sb.Append("name \"%\" and ".interp(name));
        sb.AppendLine("type '%' ['%']:".interp(obj_type.Name, obj_type.BaseType));

        foreach (FieldInfo info in field_info) {
            // Field name:
            sb.Append("%: ".interp(info.Name.PadRight(name_length_pad)));

            object field_obj = info.GetValue(obj);

            if (field_obj == null) sb.Append("(null)");
            else if (info.FieldType == typeof(string)) {
                // Print strings with quotation marks:
                sb.Append("\"%\"".interp((string)info.GetValue(obj)));
            } else if (typeof(IEnumerable).IsAssignableFrom(info.FieldType)) {
                IEnumerable enumerable         = (IEnumerable)info.GetValue(obj);
                List<string> values_as_strings = new();

                foreach (object it in enumerable) {
                    if      (it is string) values_as_strings.Add("\"%\"".interp((string)it));
                    else if (it is char)   values_as_strings.Add("'%'"  .interp((char)  it));
                    else                   values_as_strings.Add(it.ToString());
                }

                sb.Append("{ ");
                sb.AppendJoin("; ", values_as_strings);
                sb.Append(" }");
            } else {
                // Try printing the value with ToString():
                sb.Append(info.GetValue(obj).ToString());
            }

            // Field type:
            //sb.Append("  [type: %]".interp(info.FieldType.Name));
            sb.AppendLine();
        }

        return log(sb.ToString(), caller_file_path, caller_proc_name, caller_line_num);
    }

    #region Overloads
    public static Logging_Info log(object message_obj, LogTarget targets, LogLevel level,
                          [CallerFilePath] string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) =>
        log(message_obj.ToString(), targets, level, caller_file_path, caller_proc_name, caller_line_num);

    // No extra args:
    public static Logging_Info log(object message_obj, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) => log(message_obj, options.default_targets, options.default_level,                                      caller_file_path, caller_proc_name, caller_line_num);
    public static Logging_Info log(string message, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) => log(message, options.default_targets, options.default_level, 
                                                                            caller_file_path, caller_proc_name, caller_line_num);
    public static Logging_Info log_nocaller(string message) => log_nocaller(message, options.default_targets, options.default_level);

    public static Logging_Info log_warn(object message_obj,
    [CallerFilePath]   string caller_file_path = null,
    [CallerMemberName] string caller_proc_name = null,
    [CallerLineNumber] int caller_line_num = -1) => log(message_obj, options.default_targets, LogLevel.Warning,
                                                        caller_file_path, caller_proc_name, caller_line_num);
    public static Logging_Info log_warn(string message,
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) => log(message, options.default_targets, LogLevel.Warning,
                                                                            caller_file_path, caller_proc_name, caller_line_num);
    public static Logging_Info log_warn_nocaller(string message) => log_nocaller(message, options.default_targets, LogLevel.Warning);

    public static Logging_Info log_error(object message_obj,
    [CallerFilePath]   string caller_file_path = null,
    [CallerMemberName] string caller_proc_name = null,
    [CallerLineNumber] int caller_line_num = -1) => log(message_obj, options.default_targets, LogLevel.Error,
                                                        caller_file_path, caller_proc_name, caller_line_num);
    public static Logging_Info log_error(string message,
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) => log(message, options.default_targets, LogLevel.Error,
                                                                            caller_file_path, caller_proc_name, caller_line_num);
    public static Logging_Info log_error_nocaller(string message) => log_nocaller(message, options.default_targets, LogLevel.Error);

    // LogLevel arg:
    public static Logging_Info log(string message, LogLevel level, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) =>
        log(message, options.default_targets, level, 
                          caller_file_path, caller_proc_name, caller_line_num);
    public static Logging_Info log(object message_obj, LogLevel level, 
                          [CallerFilePath]   string caller_file_path = null,
                          [CallerMemberName] string caller_proc_name = null,
                          [CallerLineNumber] int caller_line_num = -1) =>
    log(message_obj, options.default_targets, level, 
                          caller_file_path, caller_proc_name, caller_line_num);

    public static Logging_Info log_nocaller(string message, LogLevel level) => log_nocaller(message, options.default_targets, level);
    public static Logging_Info log_nocaller(object message_obj, LogLevel level) => log_nocaller(message_obj, options.default_targets, level);

    public static Logging_Info log_dump_obj_with_name(object obj, string name,
                                   [CallerFilePath] string caller_file_path = null,
                                   [CallerMemberName] string caller_proc_name = null,
                                   [CallerLineNumber] int caller_line_num = -1) =>
        log_dump_obj_internal(obj, name, caller_file_path, caller_proc_name, caller_line_num);

    public static Logging_Info log_dump_obj(object obj,
                                   [CallerFilePath] string caller_file_path = null,
                                   [CallerMemberName] string caller_proc_name = null,
                                   [CallerLineNumber] int caller_line_num = -1) =>
        log_dump_obj_internal(obj, null, caller_file_path, caller_proc_name, caller_line_num);
    #endregion

    #region XZShared
#if XZSHARED && UNITY
    public static UnityEngine.Color XZ_GetColorForLogLevel(LogLevel level) {
        if (level.HasFlag(LogLevel.Error))   return "#EF5350".hex_to_unity_color();
        if (level.HasFlag(LogLevel.Warning)) return "#FB8C00".hex_to_unity_color();

        if (level.HasFlag(LogLevel.IO))      return "#388E3C".hex_to_unity_color();
        if (level.HasFlag(LogLevel.Debug))   return "#2196F3".hex_to_unity_color();

        return "#262626".hex_to_unity_color();
    }
#endif
#if XZSHARED

    static List<XZ_LogFunction_Signature> XZ_GetLogFunctions(LogTarget targets) {
        List<XZ_LogFunction_Signature> list = new();
        if (targets.HasFlag(LogTarget.IngameConsole)) list.Add(DebugConsole.write_line);

        return list;
    }
#endif
    #endregion

    #region Unity
#if UNITY
    public static LogLevel loglevel_from_unity_logtype(UnityEngine.LogType unity_logtype) {
        switch (unity_logtype) {
            default:
            case UnityEngine.LogType.Log:       return LogLevel.Info;
            case UnityEngine.LogType.Error:
            case UnityEngine.LogType.Exception:
            case UnityEngine.LogType.Assert:    return LogLevel.Error;
            case UnityEngine.LogType.Warning:   return LogLevel.Warning;
        }
    }

    static Action<object> UNITY_GetLogLevelFunction(LogLevel level) {
        switch (level) {
            default:
            case LogLevel.Info:    return UnityEngine.Debug.Log;
            case LogLevel.Warning: return UnityEngine.Debug.LogWarning;
            case LogLevel.Error:   return UnityEngine.Debug.LogError;
        }
    }
    static bool UNITY_Log(string s, LogLevel level) {
        Action<object> action_to_call = UNITY_GetLogLevelFunction(level);
        action_to_call.Invoke(s);
        return true;
    }
#endif
    #endregion
}