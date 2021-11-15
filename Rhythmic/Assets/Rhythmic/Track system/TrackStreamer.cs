using System.Collections.Generic;
using UnityEngine;
using static Logger;

public enum StreamCmd
{
    INIT,                 // Stream all measures until horizon for song startup.
    STREAM_TRACK_HORIZON, // Stream a specific track from current Clock bar until horizon.
    STREAM_TRACK_AMOUNT,  // Stream a specific track from given bar until requested amount.
    STREAM_TRACK_RANGE,   // Stream a specific track from given bar until requested bar(s).
    UNSTREAM_RECYCLE,     // Recycle a specific track's measure (preferred during streaming).
    UNSTREAM_DESTROY      // Destroy a specific track's measure.
}

public class TrackStreamer : MonoBehaviour
{
    Game Game = Game.Instance;
    public TrackSystem TrackSystem;
    Track[] tracks;

    public void SetupTrackStreamer(TrackSystem track_system)
    {
        TrackSystem = track_system;
        tracks = track_system.tracks;

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
            case StreamCmd.STREAM_TRACK_AMOUNT:
            case StreamCmd.STREAM_TRACK_RANGE:
                STREAMER_Stream(cmd, args); break;
            case StreamCmd.UNSTREAM_RECYCLE:
                STREAMER_Recycle(args); break;
            case StreamCmd.UNSTREAM_DESTROY:
                STREAMER_Destroy(args); break;

            default:
                LogE("Incorrect command! (%)".TM(this), cmd); break;
        }
    }

    public bool is_initialized = false;
    void STREAMER_Init(int[] args = null)
    {
        // TODO: Sanity checks?
        if (is_initialized && LogW("Initialization had already occured. Ignoring.".TM(this))) return;

        int start = 0;
        int horizon = Variables.STREAM_HorizonMeasures; //  Get the horizon...

        if (args != null && args.Length == 0)
        {
            if (args.Length > 0) start = args[0];
            if (args.Length > 1) horizon = args[1];
        }

        // ...



        is_initialized = true;
    }

    void STREAMER_Stream(StreamCmd cmd, params int[] args)
    {
        if ((args == null || args.Length == 0))
        {
            LogE("No args were given. (format: [T], [from], [amount/to], [to...)".TM(this));
            return;
        }

        
    }

    void STREAMER_Recycle(params int[] args)
    {

    }

    void STREAMER_Destroy(params int[] args)
    {

    }

}