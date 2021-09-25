using System.Collections.Generic;
using System.IO;
using UnityEngine;

// We might want to make this class a regular class, or perhaps even a struct in RELEASE builds.  // Incomplete
public class GameVariables : MonoBehaviour
{
    public int beat_ticks = 480;
    public int bar_ticks = 1920;

    [Header("AMP properties")]
    public List<string> AMP_song_lookup_paths = new List<string>
    {
        "C:/amp_ps3/songs"
    };

    [Header("RHX properties")]
    public List<string> RHX_song_lookup_paths = new List<string>
    {
        "<data>/rhx_songs",
        "C:/rhx_songs"
    };

    [Header("Common game variables")]
    /// <summary>Tunnel mode transforms the highway into a tunnel by rotating tracks around 360 degrees.</summary>
    public bool tunnel_mode = false;

    [Header("Track system")]
    /// <summary>The amount of milliseconds to wait between track instantiations.</summary>
    public float inst_delay_ms = 0;
}