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
        STREAMER_RegisterCommands();

        this.track_system = track_system;
        tracks = track_system.tracks;
        song = song_system.song;
        clock = song_system.clock;

        STREAMER_Init();
    }

    public bool is_initialized = false;
    void STREAMER_Init()
    {
        // TODO: Sanity checks?
        if (is_initialized && LogW("Initialization had already occured. Ignoring.".TM(this))) return;

        STREAMER_StreamRangeHorizon(-1, clock.bar);

        is_initialized = true;
    }

    Stack<TrackSection> recycled = new Stack<TrackSection>();

    public void STREAMER_Stream(int track, int measure)
    {
        int t_from = (track == -1) ? 0 : track;
        int t_to = (track == -1) ? track_system.track_count : t_from;
        for (int t = t_from; t < t_to; ++t)
        {
            // ...
            TrackSection m;
            if (recycled.Count == 0)
                m = TrackSection.CreateTrackSection(tracks[t], measure);
            else
                m = recycled.Pop().Unrecycle(tracks[t], measure);

            if (!m) LogE("Did not get a track - WTF!".TM(this));

            tracks[t].sections[measure] = m;
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
        STREAMER_StreamRange(track, from, to);
    }

    public void STREAMER_StreamRecycle(int track, int measure)
    {
        int t_from = (track == -1) ? 0 : track;
        int t_to = (track == -1) ? track_system.track_count : t_from;
        for (int t = t_from; t < t_to; ++t)
        {
            TrackSection it = tracks[t].sections[measure];
            recycled.Push(it.Recycle());
            tracks[t].sections[measure] = null;
        }
    }
    public void STREAMER_StreamRecycleRange(int track, int from, int to)
    {
        for (int i = from; i < to; ++i)
            STREAMER_StreamRecycle(track, i);
    }

    public void STREAMER_StreamDestroy(int track, int measure)
    {
        int t_from = (track == -1) ? 0 : track;
        int t_to = (track == -1) ? track_system.track_count : t_from;
        for (int t = t_from; t < t_to; ++t)
            Destroy(tracks[t].sections[measure].gameObject);
    }
    public void STREAMER_StreamDestroyRange(int track, int from, int to)
    {
        for (int i = from; i < to; ++i)
            STREAMER_StreamDestroy(track, i);
    }

    #region Console commands
    void STREAMER_RegisterCommands()
    {
        DebugConsole cmd = DebugConsole.Instance;
        if (!cmd) LogW("No console!".TM(this));

        cmd.RegisterCommand(cmd_stream);
        cmd.RegisterCommand(cmd_stream_range);
        cmd.RegisterCommand(cmd_stream_horizon);

        cmd.RegisterCommand(cmd_stream_recycle);
        cmd.RegisterCommand(cmd_stream_recycle_range);
        cmd.RegisterCommand(cmd_stream_destroy);
        cmd.RegisterCommand(cmd_stream_destroy_range);
    }

    public static void cmd_stream(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        Instance?.STREAMER_Stream(args[0].ParseInt(), args[1].ParseInt());
    }
    public static void cmd_stream_range(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        Instance?.STREAMER_StreamRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    public static void cmd_stream_horizon(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure_from]")) return;
        Instance?.STREAMER_StreamRangeHorizon(args[0].ParseInt(), args[1].ParseInt());
    }

    public static void cmd_stream_recycle(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        Instance?.STREAMER_StreamRecycle(args[0].ParseInt(), args[1].ParseInt());
    }
    public static void cmd_stream_recycle_range(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        Instance?.STREAMER_StreamRecycleRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    public static void cmd_stream_destroy(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        Instance?.STREAMER_StreamDestroy(args[0].ParseInt(), args[1].ParseInt());
    }
    public static void cmd_stream_destroy_range(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        Instance?.STREAMER_StreamDestroyRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    #endregion
}