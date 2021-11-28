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

        next_sections = new int[track_count][];
        next_notes = new Song_Note[track_count];

        // Create streamer:
        streamer = gameObject.AddComponent<TrackStreamer>();
        streamer.SetupTrackStreamer(this);
    }

    // -2: never played a track yet, -1: failed a catch
    public int track_being_played = -2;

    public int[][] next_sections;
    public Song_Note[] next_notes;

    public string DEBUG_NextSectionIDsToString() {
        string s_next_sect_ids = null;
        for (int i = 0; i < track_count; ++i) {
            int[] next_array = next_sections[i];
            s_next_sect_ids += '(';
            for (int x = 0; x < next_array.Length; ++x) {
                int sect_id = next_array[x];
                TrackSection sect = tracks[i].sections[x];

                string s = sect_id.ToString();
                if (sect) s = s.AddColor(sect.track.info.instrument.color);
                s_next_sect_ids += s;

                if (x < next_array.Length - 1) s_next_sect_ids += ' ';
            }
            s_next_sect_ids += ") ";
        }

        return s_next_sect_ids;
    }

    public int GetNextSectionID(int track_id) {
        for (int i = 0; i < next_sections[track_id].Length; ++i) {
            int id = next_sections[track_id][i];
            if (id != -1) return i;
        }
        return -1;
    }

    public void SetSectionsEnabled(int bar_id, int track_exclude, bool value) {
        foreach (Track t in tracks) {
            if (t.info.id == track_exclude) continue;

            t.sections[bar_id]?.SetEnabled(value);
            t.info.sections[bar_id].is_enabled = value;
        }
        FindNextSections();
    }

    // ----- //

    public void cmd_findnextsections() {
        FindNextSections();
        Log(DEBUG_NextSectionIDsToString());
    }
    public void FindNextSections() {
        for (int i = 0; i < track_count; ++i) {
            if (i != track_being_played)
                FindNextSection(i);
        }
    }
    public void FindNextSection(int id, bool find_next_note = true) {
        Track t = tracks[id];

        int result_index = 0, loop_target = song.length_bars;
        for (int i = (int)clock.bar; i < loop_target; ++i) {
            TrackSection sect = t.sections[i];
            Song_Section s = t.info.sections[i];

            if (sect) {
                if (sect.is_empty) continue;
                if (sect.is_captured) continue;
                if (!sect.is_enabled) continue;
            } else {
                if (s.is_empty) continue;
                if (s.is_captured) continue;
                if (s.is_enabled) continue;
            }

            if (result_index == 0) {
                next_sections[id] = new int[Variables.CATCHER_MaxSectionsToCapture];

                // Check if next sections can be captured on subsequent iterations: 
                loop_target = i + Variables.CATCHER_MaxSectionsToCapture;
            }

            // Assign to next_sections: 
            next_sections[id][result_index++] = i;
        }

        if (find_next_note) FindNextNote(id);
    }

    public void FindNextNotes() {
        for (int i = 0; i < track_count; ++i)
            FindNextNote(i);
    }
    public void FindNextNote(int id) {
        int section_id = -1;

        for (int i = 0; i < Variables.CATCHER_MaxSectionsToCapture; ++i) {
            section_id = next_sections[id][i];
            if (section_id == -1) continue;
            else break;
        }

        if (section_id == -1) {
            LogE("Next section missing for track %!".TM(this), id);
            return;
        }

        Song_Section sect = tracks[id].info.sections[section_id];

        if (sect.next_note_index >= sect.note_count) {
            LogE("Next note index for track % has exceeded note count! (% out of %)".TM(this), id, sect.next_note_index, sect.note_count);
            return;
        }
        if (sect.next_note_index == -1) return;

        next_notes[id] = tracks[id].info.notes[section_id][sect.next_note_index];
    }
}