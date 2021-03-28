using UnityEngine;

public static class StringExtensions
{
    // Colors:
    /// Credit: https://forum.unity.com/threads/change-color-of-a-single-word.538706/#post-6819410
    public static string AddColor(this string text, Color color, float? alpha = null)
    {
        if (alpha.HasValue) color.a = alpha.Value; // Set alpha if required

        string hex = ColorHexFromUnityColor(color); // Get color in hexadecimal format
        return $"<color={hex}>{text}</color>"; // Return the colored string
    }
    public static string AddColor(this string text, float alpha) => AddColor(text, new Color(1, 1, 1), alpha);
    public static string AddColor(this string text, float r, float g, float b, float? alpha = null) => AddColor(text, new Color(r, g, b), alpha);
    public static string ColorHexFromUnityColor(this Color unityColor) => $"#{ColorUtility.ToHtmlStringRGBA(unityColor)}";

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
    public static string AlignSpaces(this string text, int total_text_length, int amount)
    {
        int space_count = amount - total_text_length;
        string s = "";
        for (int i = 0; i < space_count; i++) s += ' ';

        return s + text;
    }
}