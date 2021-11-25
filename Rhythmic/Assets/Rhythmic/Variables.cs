public static class Variables {
    public static float VARIABLES_HotReloadCheckMs = 1000;

    public static int STREAMER_HorizonMeasures = 6;
    public static bool STREAMER_AllowQueueing = true;
    public static bool STREAMER_PreferQueueing = true;
    public static float STREAMER_QueueDelay = 100; // -1 means automatically decide based on framerate / frametimes

    public static int beat_ticks = 480;
    public static int bar_ticks = 1920;
    public static float UNITS_MetersPerSecond = 16f;

    public static float TRACK_Width = 3.7f;
    public static float TRACK_Height = 0.1f;
    public static int TRACK_LaneCount = 4;

    public static bool TRACKSWITCH_SlamEnabled = true;
    public static int TRACKSWITCH_SlamsTarget = 2;
    public static float TRACKSWITCH_SlamTimeoutMs = 200f;

    public static float NOTE_TrackPadding = 0.65f;
    public static float NOTE_Size = 1f;
}