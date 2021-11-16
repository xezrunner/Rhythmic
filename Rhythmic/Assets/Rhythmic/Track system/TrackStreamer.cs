using System.Collections.Generic;
using UnityEngine;
using static Logger;

public enum StreamCmd
{
    INIT,                 // Stream all measures until horizon for song startup.
    STREAM_TRACK_HORIZON, // Stream a specific track from current Clock bar until horizon.
    STREAM_TRACK_RANGE,   // Stream a specific track from given bar until requested bar(s).
    STREAM_TRACK_MEASURE, // Stream in a specific track's measure.
    UNSTREAM_RECYCLE,     // Recycle a specific track's measure (preferred during streaming).
    UNSTREAM_DESTROY      // Destroy a specific track's measure.
}

public class TrackStreamer : MonoBehaviour
{
    Game Game = Game.Instance;
    SongSystem song_system = SongSystem.Instance;
    Song song;
    Clock clock;

    public TrackSystem track_system;
    Track[] tracks;

    public void SetupTrackStreamer(TrackSystem track_system)
    {
        this.track_system = track_system;
        tracks = track_system.tracks;
        song = song_system.song;
        clock = song_system.clock;

        STREAMER_Command(StreamCmd.INIT);
    }

    /// <summary>
    /// args format: [T], [from], [to...)<br/>
    /// - [T]: specific id or -1 for all tracks<br/>
    /// - [from]: what track to stream in from<br/>
    /// - [to...): multiple targets allowed
    /// </summary>
    public void STREAMER_Command(StreamCmd cmd, params int[] args)
    {
        switch (cmd)
        {
            case StreamCmd.INIT:
                STREAMER_Init(args); break;
            case StreamCmd.STREAM_TRACK_HORIZON:
            case StreamCmd.STREAM_TRACK_RANGE:
            case StreamCmd.STREAM_TRACK_MEASURE:
                STREAMER_Stream(cmd, args); break;
            case StreamCmd.UNSTREAM_RECYCLE:
                STREAMER_Recycle(args); break;
            case StreamCmd.UNSTREAM_DESTROY:
                STREAMER_Destroy(args); break;

            default:
                LogW("Incorrect command! (%)".TM(this), cmd); break;
        }
    }

    public bool is_initialized = false;
    void STREAMER_Init(int[] args = null)
    {
        // TODO: Sanity checks?
        if (is_initialized && LogW("Initialization had already occured. Ignoring.".TM(this))) return;

        int start = 0;
        int horizon = Variables.STREAM_HorizonMeasures;

        if (args != null && args.Length != 0)
        {
            if (args.Length > 0) start = args[0];
            if (args.Length > 1) horizon = args[1];
        }

        STREAMER_Command(StreamCmd.STREAM_TRACK_HORIZON, -1, start, horizon);

        is_initialized = true;
    }

    void STREAMER_Stream(StreamCmd cmd, params int[] args)
    {
        if (args == null || args.Length == 0)
        {
            LogW("No args were given. (format: [T], [from], [amount/to], [to...)".TM(this));
            return;
        }

        int track = args[0];
        int from = 0;
        int to = -1;
        if (args.Length > 1) from = args[1];
        if (args.Length > 2) to = args[2];

        switch (cmd)
        {
            case StreamCmd.STREAM_TRACK_HORIZON:
                {
                    if (args.Length > 1) from = args[1];
                    else from = clock.bar;

                    if (args.Length > 2) to = args[2];
                    else to = Variables.STREAM_HorizonMeasures;

                    goto case StreamCmd.STREAM_TRACK_RANGE;
                }
            case StreamCmd.STREAM_TRACK_RANGE:
                {
                    int t = (track == -1) ? 0 : track;
                    for (; t < ((track == -1) ? track_system.track_count : track + 1); ++t)
                    {
                        for (int i = from; i < (to == -1 ? song.length_bars : to); ++i)
                            STREAMER_Command(StreamCmd.STREAM_TRACK_MEASURE, t, i);
                    }
                    break;
                }
            case StreamCmd.STREAM_TRACK_MEASURE:
                {
                    int t = (track == -1) ? 0 : track;
                    for (; t < ((track == -1) ? track_system.track_count : track + 1); ++t)
                    {
                        for (int i = 1; i < args.Length; ++i)
                        {
                            // TODO: Stream the measure! - handle recyclations!
                            TrackSection.CreateTrackSection(tracks[t], args[i]);
                        }
                    }
                        break;
                }
            default: LogW("Invalid command: %".TM(this), cmd); break;
        }
    }

    void STREAMER_Recycle(params int[] args)
    {

    }

    void STREAMER_Destroy(params int[] args)
    {

    }

}