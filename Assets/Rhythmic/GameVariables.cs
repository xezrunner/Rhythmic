using System.Collections.Generic;
using System.IO;
using UnityEngine;

// We might want to make this class a regular class, or perhaps even a struct in RELEASE builds.  // Incomplete
public class GameVariables : MonoBehaviour
{
    public int beat_ticks = 480;
    public int bar_ticks = 1920;

    [Header("Common game variables")]
    /// <summary>Tunnel mode transforms the highway into a tunnel by rotating tracks around 360 degrees.</summary>
    public bool tunnel_mode = false;

    [Header("Track system")]

    [Header("Track streamer")]
    /// <summary>The amount of track measures we should see / stream ahead.</summary>
    public int horizon_bars = 10;
    /// <summary>The amount of milliseconds to wait between track measure instantiations.</summary>
    public float stream_inst_delay_ms = 60;

    // ----- //

    [Header("AMP properties")]
    public List<string> AMP_song_lookup_paths = new List<string>
    {
        "G:/amp_ps3/songs"
    };

    [Header("RHX properties")]
    public List<string> RHX_song_lookup_paths = new List<string>
    {
        "<data>/rhx_songs",
        "C:/rhx_songs"
    };
}