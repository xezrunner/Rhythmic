using System.Globalization;
using System.Reflection;
using UnityEngine;

public class Colors : MonoBehaviour {
    // Editor:
    [Header("For: ConvertToFloatColor()")]
    public Vector4 Color = new Vector4(0, 0, 0, 255);
    [Header("For: ConvertHexToColor()")]
    public string Input;
    [Header("For: GetColorForCLogType()")]
    public CLogType LogType;

    // Functionality:

    public static Color RGBToFloat(int r, int g, int b, int a = 255) {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Color ConvertToFloatColor(Color color) {
        // Convert color to 0-1 color, if required
        // TODO: hacky!
        bool requiresConversion = color.r > 1 || color.g > 1 || color.b > 1;
        if (!requiresConversion) return color;

        return new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
    }
    public static Color ConvertHexToColor(string hex, bool convertToFloat = true) {
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

    // Global colors:

    // ----- Logger & Console -----
    public static Color GetColorForCLogType(CLogType logType) {
        FieldInfo field = typeof(Colors).GetField(logType.ToString(), BindingFlags.Public | BindingFlags.Static);
        Color color = (Color)field.GetValue(null);

        return color;

        /* TODO: unneccesary (?)
        switch (logType)
        {
            default: return UNKNOWN;
            case CLogType.Info: return Info;
            case CLogType.Unimportant: return Unimportant;
            case CLogType.Warning: return Warning;
            case CLogType.Error: return Error;
            case CLogType.Caution: return Caution;
            case CLogType.Network: return Network;
            case CLogType.IO: return IO;
            case CLogType.Application: return Application;
        }
        */
    }

    // TODO: brighten up some of these!
    public static Color Default = ConvertHexToColor("#f0f0f0");
    public static Color DebugMenuSelection = ConvertHexToColor("#03A9F4");

    public static Color Info = ConvertHexToColor("#f0f0f0");
    public static Color Unimportant = ConvertHexToColor("#7Bf0f0f0");
    public static Color Warning = ConvertHexToColor("#ef6c00");
    public static Color Error = ConvertHexToColor("#ff1744");
    public static Color Caution = ConvertHexToColor("#F50057");
    public static Color Network = ConvertHexToColor("#4CAF50");
    public static Color IO = ConvertHexToColor("#FFD600");
    public static Color Application = ConvertHexToColor("#03A9F4");
    public static Color UNKNOWN = ConvertHexToColor("#4A148C");
}
