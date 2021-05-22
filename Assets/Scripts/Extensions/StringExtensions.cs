using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class StringExtensions
{
    // Colors:
    /// Credit: https://forum.unity.com/threads/change-color-of-a-single-word.538706/#post-6819410
    public static string ColorHexFromUnityColor(this Color unityColor) => $"#{ColorUtility.ToHtmlStringRGBA(unityColor)}";
    public static string AddColor(this string text, Color color, float? alpha = null)
    {
        if (alpha.HasValue) color.a = alpha.Value; // Set alpha if required

        string hex = ColorHexFromUnityColor(color); // Get color in hexadecimal format
        return $"<color={hex}>{text}</color>"; // Return the colored string
    }
    public static string AddColor(this string text, float alpha) => AddColor(text, new Color(1, 1, 1), alpha);
    public static string AddColor(this string text, float r, float g, float b, float? alpha = null) => AddColor(text, new Color(r, g, b), alpha);
    public static string ClearColors(this string text)
    {
        //<color=#03A9F4FF> && </color>
        while (text.Contains("<color=#"))
        {
            int code_start_index = text.IndexOf("<color=#");
            text = text.Remove(code_start_index, "<color=#000000FF>".Length);
        }

        while (text.Contains("</color>"))
        {
            int code_start_index = text.IndexOf("</color>");
            text = text.Remove(code_start_index, "</color>".Length);
        }

        return text;
    }

    // Bold, underline, etc...
    public static string Bold(this string text) => $"<b>{text}</b>";
    public static string Underline(this string text) => $"<u>{text}</u>";
    public static string Italic(this string text) => $"<i>{text}</i>";
    // TODO: Clear___() functions for these too?

    // Max lines:
    public static string MaxLines(this string text, int maxLines)
    {
        string[] lines = text.Split('\n');
        int lineCount = lines.Length - 1;
        if (lineCount > maxLines)
        {
            int lineDiff = Mathf.Abs(maxLines - lineCount);
            string[] newLines = new string[maxLines + 1]; // newline at end!
            for (int i = lineDiff; i < lineCount; i++) // Remove lines from start to keep max line count
                newLines[i - lineDiff] = lines[i];

            text = string.Join("\n", newLines);
        }
        return text;
    }

    // Monoscape:
    // Trebuchet MS: ~8.5
    public static string Monoscape(this string text, float width = 9.5f) =>
        $"<mspace={width}>{text}</mspace>";

    // Align a text by number of spaces.
    // Note! For best results, text should be monospace!
    public static string AlignSpaces(this string text, int total_text_length, int max)
    {
        int space_count = max - total_text_length;
        string s = "";
        for (int i = 0; i < space_count; i++)
            s += ' ';

        return s + text;

    }

    public static string AlignSpaces_New(this string text, int left_length, int right_target, bool after = false)
    {
        int space_count = right_target - text.Length;
        string s = "";
        for (; space_count > 0; --space_count) s += ' ';

        if (!after)
            return s + text;
        else
            return text + s;
    }

    /// Not sure about the naming for these... the shorter ones seem more convenient.
    // TODO: Default colors

    //public static string Type(this string text, object type = null)
    public static string T(this string text, object type = null, bool standalone = true) => T(text, Colors.Unimportant, type, standalone);
    public static string T(this string text, Color color, object type = null, bool standalone = true)
    {
        if (type != null)
            return $"{(type.GetType() == typeof(string) ? (string)type : type.GetType().Name)}{(standalone ? ": " : "")}".AddColor(color) + text;
        else return text;
    }

    //public static string Method(this string text, [CallerMemberName] string methodName = null)
    public static string M(this string text, [CallerMemberName] string methodName = null) => M(text, Colors.Application, methodName);
    public static string M(this string text, Color color, [CallerMemberName] string methodName = null)
    {
        if (methodName != null)
            return $"{methodName}(): ".AddColor(color) + text;
        else return text;
    }

    //public static string TypeMethod(this string text, object type = null, [CallerMemberName] string methodName = null)
    public static string TM(this string text, object type = null, [CallerMemberName] string methodName = null) => TM(text, Colors.Application, type, methodName);
    public static string TM(this string text, Color color, object type = null, [CallerMemberName] string methodName = null)
    {
        if (type != null && (methodName == null || methodName == ""))
            return text.T(color, type);
        else if (type == null && (methodName != null && methodName != ""))
            return text.M(color, methodName);
        else if (type != null && (methodName != null && methodName != ""))
            return $"{T("", type, false)}/" + $"{methodName}(): ".AddColor(color) + text;
        else
        { Logger.LogMethodE("WTF", "StringExts", "TypeMethod"); return text; }
    }

    // Parsing:
    public static bool ParseBool(this string text)
    {
        // Try parsing as boolean:
        bool bool_result = false;
        if (bool.TryParse(text, out bool_result)) return bool_result;

        // Try parsing as integer: 
        int int_result = -1;
        if (int.TryParse(text, out int_result)) return (int_result == 1);

        // Try parsing common bool-like strings:
        string[] s_true = { "enabled", "yes", "y", "t" };
        string[] s_false = { "disabled", "no", "n", "f" };

        if (s_true.Contains(text.ToLower())) return true;
        else if (s_false.Contains(text.ToLower())) return false;

        Logger.LogW("could not parse the string '%' to boolean. Returning false.".T("StringExts"), text);
        return false;
    }
    public static int ParseInt(this string text) => int.Parse(text);
    public static float ParseFloat(this string text) => float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);

    public static string RemoveExt(this string text, string ext = null)
    {
        if (ext != null && ext != "") return text.Replace(ext, "");
        else if (text.Contains("."))
        {
            for (int i = text.Length - 1; i > 0; --i)
            {
                if (text[i] == '.')
                    return text.Substring(0, i);
            }
        }
        return text;
    }

    // TODO: We should also consider spaces as empty, potentially. Perhaps controllable?
    public static bool IsEmpty(this string text) => (text == null || text == "");
}

public static class CharExtensions
{
    public static bool IsWhitespace(this char c) => (c == ' ' || c == '\r' || c == '\n' || c == '\t');
    public static bool IsNewline(this char c) => (c == '\r' || c == '\n');
}