#undef DEBUG_CHECKS

using System.Text;

using static Logging;

public static partial class TextExtensions {
    public static string remove_file_ext(this string text) => text[.. text.LastIndexOf('.')];

    public static char INTERP_CHAR = '%';
    public const int   INTERP_BUFFER = 256; // @ Optimization

    public static string interp(this string text, params object[] args) {
        string[] args_as_strings = new string[args.Length];
        for (int i = 0; i < args.Length; ++i) args_as_strings[i] = args[i]?.ToString();
        return interp(text, args_as_strings);
    }
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

    public static bool is_empty(this string text) {
        return text == null || text == "";
    }

    // TODO: Make these safe (?)
    public static int   as_int  (this string text) => int.Parse(text);
    public static float as_float(this string text) => float.Parse(text);
    public static bool  as_bool (this string text) {
        switch (text.ToLower()) {
            case "0":
            case "false":
            case "n":
            case "no":    
                return false;
            case "1":
            case "true":
            case "y":
            case "yes":
                return true;
            default: {
                log_warning("expected boolean - returning false.");
                return false;
            }
        }
    }

#if UNITY
    public static string bold     (this string text) => $"<b>{text}</b>";
    public static string underline(this string text) => $"<u>{text}</u>";
    public static string italic   (this string text) => $"<i>{text}</i>";
    public static string color    (this string text, string color_hex) {
        if (color_hex[0] != '#') color_hex.Insert(0, "#");
        return $"<color={color_hex}>{text}</color>";
    }
    public static string color    (this string text, UnityEngine.Color unity_color, float alpha = -1f) {
        if (alpha >= 0f) unity_color.a = alpha;
        string hex = unity_color_to_hex(unity_color);
        return color(text, hex);
    }

    public static string unity_color_to_hex(this UnityEngine.Color color) {
        string result = UnityEngine.ColorUtility.ToHtmlStringRGBA(color);
        return $"#{result}";
    }
    public static UnityEngine.Color hex_to_unity_color(this string hex) {
        UnityEngine.Color color = new(1f, 0f, 0f, 1f);
        UnityEngine.ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
#endif
}