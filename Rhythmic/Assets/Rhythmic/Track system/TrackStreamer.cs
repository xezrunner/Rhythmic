using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Logger;

public class TrackStreamer : MonoBehaviour {
    public static TrackStreamer Instance;

    DebugSystem debug_system = DebugSystem.Instance;
    SongSystem song_system = SongSystem.Instance;
    AudioSystem audio_system;

    Song song;
    Clock clock;

    public TrackSystem track_system;
    Track[] tracks;

    void Start() {
        debug_system = DebugSystem.Instance;
        debug_system.SwitchToComponent(typeof(TrackStreamerDebugCom));

        STREAMER_RegisterCommands();
    }

    public void SetupTrackStreamer(TrackSystem track_system) {
        if (Instance && LogE("A track streamer already exists! This is bad!".TM(this))) return;
        Instance = this;

        this.track_system = track_system;
        audio_system = song_system.audio_system;
        tracks = track_system.tracks;
        song = song_system.song;
        clock = song_system.clock;

        // Initialize note queues for each track:
        if (Variables.STREAMER_AllowQueueing) {
            stream_notes_queue = new Queue<QueueEntry>[track_system.track_count];
            recycle_notes_queue = new Queue<QueueEntry>[track_system.track_count];
            for (int i = 0; i < track_system.track_count; i++) {
                stream_notes_queue[i] = new Queue<QueueEntry>();
                recycle_notes_queue[i] = new Queue<QueueEntry>();
            }
        }

        STREAMER_Init();
    }

    public bool is_initialized = false;
    public void STREAMER_Init(bool destroy_before_init = false) {
        // TODO: Sanity checks?
        if (is_initialized && LogW("Initialization had already occured. Ignoring.".TM(this))) return;

        STREAMER_StreamRangeHorizon(-1, (int)clock.bar, false, false);
        UpdateHorizonClipForMeasures(0);

        track_system.FindNextSections();

        is_initialized = true;
    }

    public void STREAMER_Reload(bool destroy_before_init = false) {
        if (destroy_before_init) {
            for (int i = 0; i < song.length_bars; ++i)
                STREAMER_Destroy(-1, i, false);
        }
        STREAMER_StreamRangeHorizon(-1, (int)clock.bar - 1, false, false);
        UpdateHorizonClipForMeasures(0);
    }

    // --- Queue system --- //

    public Stack<TrackSection> recycled_sections = new Stack<TrackSection>();
    public Stack<Note> recycled_notes = new Stack<Note>();

    public Queue<QueueEntry> stream_queue = new Queue<QueueEntry>();
    public Queue<QueueEntry> recycle_queue = new Queue<QueueEntry>();
    public Queue<QueueEntry> destroy_queue = new Queue<QueueEntry>();

    public Queue<QueueEntry>[] stream_notes_queue;
    public Queue<QueueEntry>[] recycle_notes_queue;

    float queue_elapsed_ms;
    // Returns: total remainder of queue elements
    void UPDATE_HandleQueue(bool force = false) {
        if (!Variables.STREAMER_AllowQueueing) return;

        //int total_count = (stream_queue.Count + recycle_queue.Count + destroy_queue.Count);
        //if (!force && total_count == 0) return total_count; // TODO: is this worth it?

        // if (!force && !audio_system.is_playing && !clock.is_testing) return total_count;

        if (queue_elapsed_ms >= Variables.STREAMER_QueueDelay || force) {
            queue_elapsed_ms = 0;

            // TODO: These are quite odd! :
            if (stream_queue.Count > 0) {
                QueueEntry entry = stream_queue.Dequeue();
                STREAMER_Stream(entry.track, entry.measure, false);
            }
            if (recycle_queue.Count > 0) {
                QueueEntry entry = recycle_queue.Dequeue();
                STREAMER_Recycle(entry.track, entry.measure, false);
            }
            if (destroy_queue.Count > 0) {
                QueueEntry entry = destroy_queue.Dequeue();
                STREAMER_Destroy(entry.track, entry.measure, false);
            }

            for (int i = 0; i < stream_notes_queue.Length; ++i) {
                if (stream_notes_queue[i].Count == 0) continue;

                QueueEntry entry = stream_notes_queue[i].Dequeue();
                STREAMER_StreamNote(entry.track, entry.measure, entry.note, false);
            }
            for (int i = 0; i < recycle_notes_queue.Length; ++i) {
                if (recycle_notes_queue[i].Count == 0) continue;

                QueueEntry entry = recycle_notes_queue[i].Dequeue();
                STREAMER_RecycleNote(entry.track, entry.measure, entry.note, false);
            }
        }

        queue_elapsed_ms += Time.deltaTime * 1000;
    }

    // --- Queue system --- //
    // TODO: Eliminate code duplication, if possible!

    public void STREAMER_Stream(int track, int measure, bool? queue = null, bool? queue_notes = null) {
        if (track == -1) {
            for (int t = 0; t < track_system.track_count; ++t)
                STREAMER_Stream(t, measure, queue, queue_notes);
            return;
        }

        if (measure < 0 || measure >= song.length_bars) return;
        if (tracks[track].sections[measure] != null /*&& LogW("%/%: measure already exists!".TM(this), tracks[track].info.name, measure)*/) return;

        if (Variables.STREAMER_AllowQueueing) {
            // Enqueue if: 
            // - 'queue' has a value and is true
            // - 'queue' has no value, but queueing is preferred by policy
            if ((queue.HasValue && queue.Value) || (!queue.HasValue && Variables.STREAMER_PreferQueueing)) {
                if (!stream_queue.Any(e => e.track == track && e.measure == measure))
                    stream_queue.Enqueue(new QueueEntry(track, measure));
                return;
            }
        }

        TrackSection m;
        if (recycled_sections.Count == 0)
            m = TrackSection.CreateTrackSection(tracks[track], measure);
        else
            m = recycled_sections.Pop().Setup(tracks[track], measure);

        if (!m) LogE("Did not get a track - WTF!".TM(this));

        tracks[track].sections[measure] = m;

        // Stream notes:
        if (tracks[track].info.notes[measure] != null)
            STREAMER_StreamNote(track, measure, -1, queue_notes);
    }
    public void STREAMER_StreamRange(int track, int from, int to, bool? queue = null, bool? queue_notes = null) {
        for (int i = from; i < to; ++i)
            STREAMER_Stream(track, i, queue, queue_notes);
    }
    public void STREAMER_StreamRangeHorizon(int track, int from, bool? queue = null, bool? queue_notes = null) {
        int to = (int)clock.bar + Variables.STREAMER_HorizonMeasures;
        STREAMER_StreamRange(track, from, to, queue, queue_notes);
    }

    public void STREAMER_StreamNote(int track, int measure, int note, bool? queue = null) {
        if (track == -1) {
            for (int t = 0; t < track_system.track_count; ++t)
                STREAMER_StreamNote(t, measure, note, queue);
            return;
        }

        if (tracks[track].sections[measure] == null) return;
        if (tracks[track].info.notes[measure] == null && LogW("%/%: measure has no notes!".TM(this), tracks[track].info.name, measure)) return;

        if (note == -1) {
            for (int n = 0; n < tracks[track].info.notes[measure].Count; ++n)
                STREAMER_StreamNote(track, measure, n, queue);
            return;
        }

        if (Variables.STREAMER_AllowQueueing) {
            // Enqueue if: 
            // - 'queue' has a value and is true
            // - 'queue' has no value, but queueing is preferred by policy
            if ((queue.HasValue && queue.Value) || (!queue.HasValue && Variables.STREAMER_PreferQueueing)) {
                if (!stream_notes_queue[track].Any(e => e.measure == measure && e.note == note))
                    stream_notes_queue[track].Enqueue(new QueueEntry(track, measure, note));
                return;
            }
        }

        TrackSection m = tracks[track].sections[measure];
        Note it;
        if (recycled_notes.Count == 0)
            it = Note.CreateNote(note, m);
        else
            it = recycled_notes.Pop().Setup(note, m);

        m.notes[note] = it;
    }

    public void STREAMER_Recycle(int track, int measure, bool? queue = null) {
        if (track == -1) {
            int t_from = (track == -1) ? 0 : track;
            int t_to = (track == -1) ? track_system.track_count : track + 1;
            for (int t = t_from; t < t_to; ++t)
                STREAMER_Recycle(t, measure, queue);
            return;
        }

        TrackSection it = tracks[track].sections[measure];
        if (!it /*&& LogW("%/%: measure does not exist!".TM(this), tracks[track].info.name, measure)*/) return;

        if (Variables.STREAMER_AllowQueueing) {
            // Enqueue if: 
            // - 'queue' has a value and is true
            // - 'queue' has no value, but queueing is preferred by policy
            if ((queue.HasValue && queue.Value) || (!queue.HasValue && Variables.STREAMER_PreferQueueing)) {
                if (!recycle_queue.Any(e => e.track == track && e.measure == measure))
                    recycle_queue.Enqueue(new QueueEntry(track, measure));
                return;
            }
        }

        STREAMER_RecycleNote(track, measure, -1, false);

        recycled_sections.Push(it.Recycle());
        tracks[track].sections[measure] = null;
    }
    public void STREAMER_RecycleRange(int track, int from, int to, bool? queue = null) {
        for (int i = from; i < to; ++i)
            STREAMER_Recycle(track, i, queue);
    }
    public void STREAMER_RecycleNote(int track, int measure, int note, bool? queue = null) {
        if (track == -1) {
            for (int t = 0; t < track_system.track_count; ++t)
                STREAMER_RecycleNote(t, measure, note, queue);
            return;
        }
        if (note == -1) {
            if (tracks[track].info.notes[measure] == null) return;
            for (int n = 0; n < tracks[track].info.notes[measure].Count; ++n)
                STREAMER_RecycleNote(track, measure, n, queue);
            return;
        }

        Note it = tracks[track].sections[measure]?.notes[note];
        if (!it /*&& LogW("%/%: measure does not exist!".TM(this), tracks[track].info.name, measure)*/) return;

        if (Variables.STREAMER_AllowQueueing) {
            // Enqueue if: 
            // - 'queue' has a value and is true
            // - 'queue' has no value, but queueing is preferred by policy
            if ((queue.HasValue && queue.Value) || (!queue.HasValue && Variables.STREAMER_PreferQueueing)) {
                if (!recycle_notes_queue[track].Any(e => e.measure == measure && e.note == note))
                    recycle_notes_queue[track].Enqueue(new QueueEntry(track, measure, note));
                return;
            }
        }

        recycled_notes.Push(it.Recycle());
        tracks[track].sections[measure].notes[note] = null;
    }

    public void STREAMER_Destroy(int track, int measure, bool? queue = null) {
        if (track == -1) {
            int t_from = (track == -1) ? 0 : track;
            int t_to = (track == -1) ? track_system.track_count : track + 1;
            for (int t = t_from; t < t_to; ++t)
                STREAMER_Destroy(t, measure, queue);
            return;
        }

        if (Variables.STREAMER_AllowQueueing) {
            // Enqueue if: 
            // - 'queue' has a value and is true
            // - 'queue' has no value, but queueing is preferred by policy
            if ((queue.HasValue && queue.Value) || (!queue.HasValue && Variables.STREAMER_PreferQueueing)) {
                if (!destroy_queue.Any(e => e.track == track && e.measure == measure))
                    destroy_queue.Enqueue(new QueueEntry(track, measure));
                return;
            }
        }

        Destroy(tracks[track].sections[measure]?.gameObject);
        // TODO: Why do we need these extra measures?
        if (tracks[track].sections[measure]) Destroy(tracks[track].sections[measure]);
        tracks[track].sections[measure] = null;
    }
    public void STREAMER_DestroyRange(int track, int from, int to, bool? queue = null) {
        for (int i = from; i < to; ++i)
            STREAMER_Destroy(track, i, queue);
    }

    int last_bar = 0;
    void UPDATE_Streaming() {
        int bar = (int)clock.bar;
        if (last_bar != bar) {
            STREAMER_StreamRangeHorizon(-1, bar);
            if (last_bar != 0) STREAMER_RecycleRange(-1, 0, last_bar);
            if (last_bar != 0) STREAMER_RecycleRange(-1, (int)clock.bar + Variables.STREAMER_HorizonMeasures, song.length_bars);
            // Log("bar: % (last_bar: %)".TM(this), bar, last_bar);

            UpdateHorizonClipForMeasures(bar);

            last_bar = bar;
        }
    }

    // TODO: This is really not working right now. Figure out why!
    void UpdateHorizonClipForMeasures(int bar) {
        int horizon_id = Variables.STREAMER_HorizonMeasures - 2;

        for (int i = 0; i < track_system.track_count; ++i) {
            Track t = tracks[i];

            for (int x = bar + horizon_id - 2; x >= 0; --x) {
                TrackSection m = t.sections[x];
                if (m && m.mesh_renderer.material != t.material) m.ChangeMaterial(t.material);
                else break;
            }
        }
    }
    void HandleClipping() {
        float horizon_bar = clock.bar + Variables.STREAMER_HorizonMeasures - 2;
        float horizon_distance = song.time_units.pos_in_bar * horizon_bar;
        //Log(horizon_distance);

        Vector3 horizon_point = PathTransform.pathcreator_global.path.XZ_GetPointAtDistance(horizon_distance);
        Quaternion horizon_rot = PathTransform.pathcreator_global.path.XZ_GetRotationAtDistance(horizon_distance);
        Vector3 horizon_normal = horizon_rot * Vector3.forward;

        Debug.DrawLine(horizon_point, horizon_point + (horizon_normal), Color.red, 1000);
#if false
        {
            GameObject obj_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            obj_plane.name = "plane";
            obj_plane.transform.position = horizon_point;
            Vector3 horizon_up = PathTransform.pathcreator_global.path.XZ_GetNormalAtDistance(horizon_distance);
            obj_plane.transform.rotation = Quaternion.LookRotation(horizon_normal, horizon_up) * Quaternion.Euler(-90, 0, 0);
        }
#endif

        Vector4 result = horizon_normal.normalized;
        result.w = -Vector3.Dot(horizon_normal, horizon_point);

        // Shader.SetGlobalVector("_HorizonPoint", horizon_point);
        Shader.SetGlobalVector("_Plane", result);

        // Log("horizon_plane: %, w = %", result, result.w);
    }

    void Update() {
        UPDATE_HandleQueue();
        UPDATE_Streaming();

        HandleClipping();
    }

    #region Console commands
    void STREAMER_RegisterCommands() {
        DebugConsole cmd = DebugConsole.Instance;
        if (!cmd) LogW("No console!".TM(this));

        cmd.RegisterCommand(cmd_stream_handlequeue, "stream_hq");

        cmd.RegisterCommand(cmd_stream);
        cmd.RegisterCommand(cmd_stream_range);
        cmd.RegisterCommand(cmd_stream_horizon);

        cmd.RegisterCommand(cmd_stream_recycle, "stream_r");
        cmd.RegisterCommand(cmd_stream_recycle_range);
        cmd.RegisterCommand(cmd_stream_destroy, "stream_d");
        cmd.RegisterCommand(cmd_stream_destroy_range);
    }

    public static void cmd_stream_handlequeue() { /* while (Instance?.UPDATE_HandleQueue(true) != 0) ; */ }

    public static void cmd_stream(string[] args) {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        Instance?.STREAMER_Stream(args[0].ParseInt(), args[1].ParseInt());
    }
    public static void cmd_stream_range(string[] args) {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        Instance?.STREAMER_StreamRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    public static void cmd_stream_horizon(string[] args) {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure_from]")) return;
        Instance?.STREAMER_StreamRangeHorizon(args[0].ParseInt(), args[1].ParseInt());
    }

    public static void cmd_stream_recycle(string[] args) {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        Instance?.STREAMER_Recycle(args[0].ParseInt(), args[1].ParseInt());
    }
    public static void cmd_stream_recycle_range(string[] args) {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        Instance?.STREAMER_RecycleRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    public static void cmd_stream_destroy(string[] args) {
        if (args.Length < 2 && ConsoleLogE("You need at least 2 arguments: [track] [measure]")) return;
        Instance?.STREAMER_Destroy(args[0].ParseInt(), args[1].ParseInt());
    }
    public static void cmd_stream_destroy_range(string[] args) {
        if (args.Length < 2 && ConsoleLogE("You need at least 3 arguments: [track] [measure_from] [measure_to]")) return;
        Instance?.STREAMER_DestroyRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    }
    #endregion

    public enum QueueEntryType { Measure, Note }
    public struct QueueEntry {
        public QueueEntry(QueueEntryType type, int track, int measure, int note = -1) {
            this.type = type;
            this.track = track; this.measure = measure;
            this.note = note;
        }
        public QueueEntry(int track, int measure) {
            type = QueueEntryType.Measure;
            this.track = track; this.measure = measure;
            note = -1;
        }
        public QueueEntry(int track, int measure, int note) {
            type = QueueEntryType.Note;
            this.track = track; this.measure = measure;
            this.note = note;
        }
        public QueueEntryType type;
        public int track;
        public int measure;
        public int note;
    }
}