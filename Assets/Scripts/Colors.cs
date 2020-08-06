using UnityEngine;

public class Colors : MonoBehaviour
{
    public static Color ConvertColor(Color color)
    {
        return new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
    }
}
