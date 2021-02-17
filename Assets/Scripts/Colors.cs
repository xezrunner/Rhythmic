using System.Globalization;
using UnityEngine;

public class Colors : MonoBehaviour
{
    // Editor:
    [Header("For: ConvertToFloatColor()")]
    public Vector4 Color = new Vector4(0, 0, 0, 255);
    [Header("For: ConvertHexToColor()")]
    public string Input;

    // Functionality:
    public static Color ConvertToFloatColor(Color color)
    {
        // Convert color to 0-1 color, if required
        // TODO: hacky!
        bool requiresConversion = color.r > 1 || color.g > 1 || color.b > 1;
        if (!requiresConversion) return color;

        return new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
    }

    public static Color ConvertHexToColor(string hex, bool convertToFloat = true)
    {
        // Remove # from start, if exists
        if (hex[0] == '#') hex = hex.Substring(1);

        bool hasAlpha = hex.Length == 8;
        int[] rgbaColors = new int[4];

        for (int i = 0, c = !hasAlpha ? 0 : 2; c < hex.Length; c += 2, i++)
            rgbaColors[i] = int.Parse(hex.Substring(c, 2), NumberStyles.AllowHexSpecifier);

        // Add alpha if exists : 255
        rgbaColors[3] = hasAlpha ? int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier) : 255;

        Color finalColor = new Color(rgbaColors[0], rgbaColors[1], rgbaColors[2], rgbaColors[3]);

        if (convertToFloat) return ConvertToFloatColor(finalColor);
        else return finalColor;
    }
}
