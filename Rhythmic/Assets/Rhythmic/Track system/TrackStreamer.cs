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

    TrackSystem track_system;
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

        STREAMER_StreamRangeHorizon(-1, (int)clock.bar, false);

        is_initialized = true;
    }

    // --- Queue system --- //
    Stack<TrackSection> recycled = new Stack<TrackSection>();

    Queue<QueueEntry> stream_queue = new Queue<QueueEntry>();
    Queue<QueueEntry> recycle_queue = new Queue<QueueEntry>();
    Queue<QueueEntry> destroy_queue = new Queue<QueueEntry>();

    class QueueEntry
    {
        public QueueEntry(int track, int measure) { this.track = track; this.measure = measure; }
        public int track;
        public int measure;
    }
    void UPDATE_HandleQueue()
    {
        if (!Variables.STREAMER_AllowQueueing) return;

        if (stream_queue.Count + recycle_queue.Count + destroy_queue.Count == 0) return; // TODO: is this worth it?
        if (queue_elapsed_ms > Variables.STREAMER_QueueDelay)
        {
            queue_elapsed_ms = 0;

            QueueEntry stream_entry = (stream_queue.Count > 0) ? stream_queue.Dequeue() : null;
            QueueEntry recycle_entry = (recycle_queue.Count > 0) ? recycle_queue.Dequeue() : null;
            QueueEntry destroy_entry = (destroy_queue.Count > 0) ? destroy_queue.Dequeue() : null;

            if (stream_entry != null) STREAMER_Stream(stream_entry.track, stream_entry.measure, false);
            if (recycle_entry != null) STREAMER_Stream(recycle_entry.track, recycle_entry.measure, false);
            if (destroy_entry != null) STREAMER_Stream(destroy_entry.track, destroy_entry.measure, false);
        }

        queue_elapsed_ms += Time.deltaTime * 1000;
    }
    // --- Queue system --- //

    public void STREAMER_Stream(int track, int measure, bool? queue = null)
    {
        if (track == -1)
        {
            int t_from = (track == -1) ? 0 : track;
            int t_to = (track == -1) ? track_system.track_count : track + 1;
            for (int t = t_from; t < t_to; ++t)
                STREAMER_Stream(t, measure, queue);
            return;
        }

        if (tracks[track].sections[measure] != null && LogW("%/%: measure already exists!".TM(this), tracks[track].info.name, measure)) return;

        if (Variables.STREAMER_AllowQueueing)
        {
            // Enqueue if: 
            // - 'queue' has a value and is true
            // - 'queue' has no value, but queueing is preferred by policy
            if ((queue.HasValue && queue.Value) || (!queue.HasValue && Variables.STREAMER_PreferQueueing))
            {
                stream_queue.Enqueue(new QueueEntry(track, measure));
                return;
            }
        }

        TrackSection m;
        if (recycled.Count == 0)
            m = TrackSection.CreateTrackSection(tracks[track], measure);
        else
            m = recycled.Pop().Unrecycle(tracks[track], measure);

        if (!m) LogE("Did not get a track - WTF!".TM(this));

        tracks[track].sections[measure] = m;
    }
    public void STREAMER_StreamRange(int track, int from, int to, bool? queue = null)
    {
        for (int i = from; i < to; ++i)
            STREAMER_Stream(track, i, queue);
    }
    public void STREAMER_StreamRangeHorizon(int track, int from, bool? queue = null)
    {
        int to = (int)clock.bar + Variables.STREAMER_HorizonMeasures;
        STREAMER_StreamRange(track, from, to, queue);
    }

    public void STREAMER_Recycle(int track, int measure, bool? queue = null)
    {
        if (track == -1)
        {
            int t_from = (track == -1) ? 0 : track;
            int t_to = (track == -1) ? track_system.track_count : track + 1;
            for (int t = t_from; t < t_to; ++t)
                STREAMER_Recycle(t, measure, queue);
            return;
        }

        TrackSection it = tracks[track].sections[measure];
        if (!it && LogW("%/%: measure does not exist!".TM(this), tracks[track].info.name, measure)) return;

        if (Variables.STREAMER_AllowQueueing)
        {
            // Enqueue if: 
            // - 'queue' has a value and is true
            // - 'queue' has no value, but queueing is preferred by policy
            if ((queue.HasValue && queue.Value) || (!queue.HasValue && Variables.STREAMER_PreferQueueing))
            {
                recycle_queue.Enqueue(new QueueEntry(track, measure));
                return;
            }
        }

        recycled.Push(it.Recycle());
        tracks[track].sections[measure] = null;
    }
    public void STREAMER_RecycleRange(int track, int from, int to, bool? queue = null)
    {
        for (int i = from; i < to; ++i)
            STREAMER_Recycle(track, i, queue);
    }

    public void STREAMER_Destroy(int track, int measure, bool? queue = null)
    {
        if (track == -1)
        {
            int t_from = (track == -1) ? 0 : track;
            int t_to = (track == -1) ? track_system.track_count : track + 1;
            for (int t = t_from; t < t_to; ++t)
                STREAMER_Destroy(t, measure, queue);
            return;
        }

        if (Variables.STREAMER_AllowQueueing)
        {
            // Enqueue if: 
            // - 'queue' has a value and is true
            // - 'queue' has no value, but queueing is preferred by policy
            if ((queue.HasValue && queue.Value) || (!queue.HasValue && Variables.STREAMER_PreferQueueing))
            {
                destroy_queue.Enqueue(new QueueEntry(track, measure));
                return;
            }
        }
        Destroy(tracks[track].sections[measure].gameObject);
    }
    public void STREAMER_DestroyRange(int track, int from, int to, bool? queue = null)
    {
        for (int i = from; i < to; ++i)
            STREAMER_Destroy(track, i, queue);
    }

    float queue_elapsed_ms;

    void UPDATE_Streaming()
    {

    }

    void Update()
    {
        UPDATE_HandleQueue();
        UPDATE_Streaming();
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
        Instance?.STREAMER_Recycle(args[0].ParseInt(), args[1].ParseInt());
    }
    public static void cmd_stream_recycle_range(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        Instance?.STREAMER_RecycleRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    public static void cmd_stream_destroy(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        Instance?.STREAMER_Destroy(args[0].ParseInt(), args[1].ParseInt());
    }
    public static void cmd_stream_destroy_range(string[] args)
    {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        Instance?.STREAMER_DestroyRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    #endregion
}