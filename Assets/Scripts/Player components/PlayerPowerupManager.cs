using System;
using UnityEngine;

[Flags]
public enum PowerupType
{
    All = -2,
    UNKNOWN = -1,
    None = 0,
    Generic = 1,
    Special = 1 << 1, Special2 = 1 << 2
}

/// TODO:
/// - We will want to let songs have their own powerup possibility configurations.
public partial class PlayerPowerupManager : MonoBehaviour
{
    public static PlayerPowerupManager Instance;

    SongController SongController { get { return SongController.Instance; } }
    TrackStreamer TrackStreamer { get { return TrackStreamer.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }

    // This controls which powerups can be generated on the tracks.
    public PowerupType Configuration = PowerupType.All;

    void Awake() => Instance = this;

    public void STREAMER_GeneratePowerupMap()
    {
        for (int i = 0; i < TracksController.MainTracks_Count; ++i)
        {
            for (int x = 0; x < SongController.songLengthInMeasures; ++x)
            {
                if (x % 2 != 0) continue;
                TrackStreamer.metaMeasures[i, x].Powerup = PowerupType.Special;
            }
        }
    }
}