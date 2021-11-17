using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class TrackStreamer : MonoBehaviour
{
    public static TrackStreamer Instance;

    Game Game = Game.Instance;
    SongSystem song_system = SongSystem.Instance;
    Song song;
    Clock clock;

    public TrackSystem track_system;
    Track[] tracks;

    public void SetupTrackStreamer(TrackSystem track_system)
    {
        if (Instance && LogE("A track streamer already exists! This is bad!".TM(this))) return;
        Instance = this;

        this.track_system = track_system;
        tracks = track_system.tracks;
        song = song_system.song;
        clock = song_system.clock;

        STREAMER_Init();
    }

    void STREAMER_RegisterCommands()
    {

    }

    public bool is_initialized = false;
    void STREAMER_Init()
    {
        // TODO: Sanity checks?
        if (is_initialized && LogW("Initialization had already occured. Ignoring.".TM(this))) return;

        STREAMER_StreamRangeHorizon(-1, clock.bar);

        is_initialized = true;
    }

    public void STREAMER_Stream(int track, int measure)
    {
        int t_from = (track == -1) ? 0 : track;
        int t_to = (track == -1) ? track_system.track_count : t_from;
        for (int t = t_from; t < t_to; ++t)
        {
            // ...
            TrackSection.CreateTrackSection(tracks[t], measure);
        }
    }
    public void STREAMER_StreamRange(int track, int from, int to)
    {
        for (int i = from; i < to; ++i)
            STREAMER_Stream(track, i);
    }
    public void STREAMER_StreamRangeHorizon(int track, int from)
    {
        int to = clock.bar + Variables.STREAM_HorizonMeasures;
        for (int i = from; i < to; ++i)
            STREAMER_StreamRange(track, from, to);
    }

    public void STREAMER_UnstreamRecycle(int track, int measure)
    {

    }
    public void STREAMER_UnstreamRecycleRange(int track, int from, int to)
    {

    }
    
    public void STREAMER_UnstreamDestroy(int track, int measure)
    {

    }
    public void STREAMER_UnstreamDestroyRange(int track, int from, int to)
    {

    }

    #region Console commands
    public void cmd_stream(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        STREAMER_Stream(args[0].ParseInt(), args[1].ParseInt());
    }
    public void cmd_stream_range(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        STREAMER_StreamRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    public void cmd_stream_horizon(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure_from]")) return;
        STREAMER_StreamRangeHorizon(args[0].ParseInt(), args[1].ParseInt());
    }

    public void cmd_unstream_recycle(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        STREAMER_UnstreamRecycle(args[0].ParseInt(), args[1].ParseInt());
    }
    public void cmd_unstream_recycle_range(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        STREAMER_UnstreamRecycleRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    public void cmd_unstream_destroy(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        STREAMER_UnstreamDestroy(args[0].ParseInt(), args[1].ParseInt());
    }
    public void cmd_unstream_destroy_range(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        STREAMER_UnstreamDestroyRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    #endregion
}