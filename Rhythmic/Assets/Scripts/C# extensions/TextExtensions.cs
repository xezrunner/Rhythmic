#undef DEBUG_CHECKS

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

public enum CallerDebugInfoFlags {
        None = 0,
        FileName   = 1 << 0,
        ProcName   = 1 << 1,
        LineNumber = 1 << 2,

        FPL = FileName | ProcName | LineNumber,
        FP  = FileName | ProcName,
        FL  = FileName | LineNumber,
}

public static class TextExtensions {
    public static bool CALLER_RemoveExtFromFilenames = true;
    public static string add_caller_debug_info(this string text, CallerDebugInfoFlags flags, 
                                              [CallerFilePath]   string caller_file_path = null,
                                              [CallerMemberName] string caller_proc_name = null,
                                              [CallerLineNumber] int caller_line_num = -1) {
        if (flags == CallerDebugInfoFlags.None) return text;
                                                
        string s = null;
        if (flags.HasFlag(CallerDebugInfoFlags.FileName)) {
            // TODO: We might not want the file extension here? Option?
            string file_name = Path.GetFileName(caller_file_path);
            if (CALLER_RemoveExtFromFilenames) file_name = file_name.remove_file_ext();
            s += file_name;
        }
        if (flags.HasFlag(CallerDebugInfoFlags.ProcName)) {
            if (flags.HasFlag(CallerDebugInfoFlags.FileName)) s += "/";
            s += $"{caller_proc_name}()";
        }
        if (flags.HasFlag(CallerDebugInfoFlags.LineNumber)) {
            s += $"@{caller_line_num}";
        }

        s += $": {text}";
        return s;
    }

    public static string remove_file_ext(this string text) => text[.. text.LastIndexOf('.')];

    public static char INTERP_CHAR = '%';
    public const int INTERP_BUFFER = 256; // @ Optimization
    public static string interp(this string text, params string[] args) {
        string[] split = text.Split(INTERP_CHAR);

#if DEBUG_CHECKS
        int placeholder_count = text.ToCharArray().Count(c => c == INTERP_CHAR);
        if (placeholder_count != args.Length) throw new("Mismatching placeholder and args count!");
#endif

        StringBuilder builder = new(text.Length + INTERP_BUFFER);
        for (int i = 0; i < args.Length; ++i) {
            builder.Append(split[i]);
            builder.Append(args[i]);
        }
        for (int i = args.Length; i < split.Length; ++i) builder.Append(split[i]);

        return builder.ToString();
    }
}
