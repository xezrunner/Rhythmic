using UnityEngine;

public static class StringExtensions
{
    // Colors:
    /// Credit: https://forum.unity.com/threads/change-color-of-a-single-word.538706/#post-6819410
    public static string AddColor(this string text, float r, float g, float b, float? alpha = null) => AddColor(text, new Color(r, g, b, alpha));
    public static string AddColor(this string text, Color color, float? alpha = null)
    {
        if (alpha.HasValue) color.a = alpha.Value; // Set alpha if required

        string hex = ColorHexFromUnityColor(color); // Get color in hexadecimal format
        return $"<color={hex}>{text}</color>"; // Return the colored string
    }
    public static string ColorHexFromUnityColor(this Color unityColor) => $"#{ColorUtility.ToHtmlStringRGBA(unityColor)}";
}