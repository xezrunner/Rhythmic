using UnityEngine;
using static Logger;

public class TrackSystem : MonoBehaviour {
    public static TrackSystem Instance;

    DebugConsole console;
    Clock clock;

    void Awake() {
        clock = Clock.Instance;
    }
    void Start() {
        console = DebugConsole.Instance;
        console?.RegisterCommand(cmd_findnextsections);
    }

    Song song;
    public TrackStreamer streamer;
    public Track[] tracks;
    public int track_count;

    public int track_being_played = -1;

    public TrackSection[][] next_sections;
    public Note[] next_notes;

    public string DEBUG_NextSectionIDsToString() {
        string s_next_sect_ids = null;
        for (int i = 0; i < track_count; ++i) {
            TrackSection[] sect_array = next_sections[i];
            s_next_sect_ids += '(';
            for (int x = 0; x < sect_array.Length; ++x) {
                TrackSection sect = sect_array[x];
                if (!sect) continue;

                s_next_sect_ids += sect.id.ToString().AddColor(sect.track.info.instrument.color);
                if (x < sect_array.Length - 1) s_next_sect_ids += ' ';
            }
            s_next_sect_ids += ") ";
        }

        return s_next_sect_ids;
    }

    public void cmd_findnextsections() {
        FindNextSections();
        Log(DEBUG_NextSectionIDsToString());
    }
    public void FindNextSections() {
        for (int i = 0; i < track_count; ++i)
            FindNextSection(i);
    }
    public void FindNextSection(int id, bool find_next_note = true) {
        if (id == track_being_played) return;

        Track t = tracks[id];
        int res_index = 0, loop_target = song.length_bars;
        for (int i = (int)clock.bar; i < loop_target; ++i) {
            TrackSection s = t.sections[i];

            if (s) {
                if (s.is_empty) continue;
                if (s.is_captured) continue;
                if (!s.is_enabled) continue;
            }

            if (res_index == 0) {
                next_sections[id] = new TrackSection[Variables.CATCHER_MaxSectionsToCapture];
                loop_target = i + Variables.CATCHER_MaxSectionsToCapture; // Check if next sections can be captured on subsequent iterations.
            }
            next_sections[id][res_index++] = s;
        }

        if (find_next_note) FindNextNote(id);
    }

    public void FindNextNotes() {
        for (int i = 0; i < track_count; ++i)
            FindNextNote(i);
    }
    public void FindNextNote(int id) {
        TrackSection sect = null;
        int sect_next_id = 0; // for removal on last note

        foreach (TrackSection it in next_sections[id])
            if (it) { sect = it; break; } else ++sect_next_id;

        if (!sect) {
            LogE("Next section is missing for track %!".TM(this), id);
            return;
        }

        if (sect.next_note_index >= sect.notes.Length) {
            LogE("Next note index for track % has exceeded note count! (% out of %)".TM(this), id, sect.next_note_index, sect.notes.Length);
            return;
        }
        if (sect.next_note_index == -1) {
            // 1. Remove the last section from next sections:
            next_sections[id][sect_next_id] = null;
            // 2. Rerun this procedure.
            FindNextNote(id);
            return;
        }

        next_notes[id] = sect.notes[sect.next_note_index];
    }

    public void SetupTrackSystem(Song song) {
        Instance = this;
        this.song = song;

        // Create tracks:
        tracks = new Track[song.track_count];
        track_count = song.track_count;
        for (int i = 0; i < song.track_count; ++i) {
            GameObject obj = new GameObject(song.tracks[i].name);
            obj.transform.SetParent(transform);
            tracks[i] = new Track(this, song, i, obj.transform);
        }

        next_sections = new TrackSection[track_count][];
        next_notes = new Note[track_count];

        // Create streamer:
        streamer = gameObject.AddComponent<TrackStreamer>();
        streamer.SetupTrackStreamer(this);
    }
}