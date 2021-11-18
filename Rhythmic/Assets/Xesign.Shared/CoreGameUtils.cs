using UnityEngine;
using static Logger;

public static class CoreGameUtils
{
    public static int FRAMERATE_LowestAcceptable = 60; // Lowest accepted framerate until it's marked red

    /// <param name="vsync_mode">0 = No VSync | >0 = passes of VSync between each frame</param>
    public static void SetFramerate(int target = 200, int vsync_mode = 0)
    {
        Application.targetFrameRate = target;
        QualitySettings.vSyncCount = vsync_mode;
        Log("Framerate set: % FPS " + "(% Vsync passes)".AddColor(Colors.Unimportant), target, vsync_mode);
    }

    public static void SetTimescale(float value = 1f)
    {
        Time.timeScale = value;
        Log("Timescale set: %x", value);
    }
}