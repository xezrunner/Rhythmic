using NAudio.Midi;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Logger;

public static class AMP_SongLoader {
    public static Song LoadSong(string song_name) {
        // TODO: Revise and error checking!!!
        string moggsong_path = Path.Combine(AMP_Constants.MOGGSONG_PATH, song_name, song_name + ".moggsong");
        if (!File.Exists(moggsong_path) && LogE("The .moggsong for song % doesn't exist at '%'.".TM(nameof(AMP_SongLoader)),
            song_name, moggsong_path)) return null;
        AMP_MoggSong moggsong = new AMP_MoggSong(moggsong_path);

        string midi_path = Path.Combine(AMP_Constants.MIDI_PATH, song_name, song_name + ".mid");
        if (!File.Exists(midi_path) && LogE("The .mid for song % doesn't exist at '%'.".TM(nameof(AMP_SongLoader)),
            song_name, midi_path)) return null;
        AMP_MidiFile midifile = new AMP_MidiFile(midi_path);

        // Create Song:
        Song song = new Song() {
            name = song_name,
            path = Path.Combine(AMP_Constants.MOGGSONG_PATH, song_name),

            length_bars = moggsong.length,
            countin = moggsong.countin,
            bpm = moggsong.bpm,

            //world_name = moggsong.arena_path,
            tunnel_scale = moggsong.tunnel_scale,

            enable_order = moggsong.enable_order,
            section_bars = moggsong.section_start_bars,
            score_goals = moggsong.score_goal
        };
        song.time_units = new Song_TimeUnits(song.bpm, song.tunnel_scale); // TODO: Cleanup - perhaps there should be a setup function for Song?

        // Build tracks:
        List<Song_Track> tracks = new List<Song_Track>();
        foreach (AMP_MidiTrack midi_track in midifile.tracks) {
            string track_name_for_audio = midi_track._text.Replace(':', '_');
            string audio_path = Path.Combine(AMP_Constants.AUDIO_PATH, song_name, "audio", track_name_for_audio + ".ogg");

            if (!File.Exists(audio_path))
                LogW("The audio for song % doesn't exist at '%'.".TM(nameof(AMP_SongLoader)), song_name, audio_path);

            // TODO: refer to Song.cs/Song_Instrument!
            InstrumentType instrument = InstrumentType.UNKNOWN;
            instrument = (InstrumentType)midi_track.instrument + 1;

            Song_Track track = new Song_Track() {
                id = midi_track.id,
                name = midi_track.name,
                instrument = new Song_Instrument(instrument),
            };

            // TODO: Performance!
            List<NoteOnEvent> note_events_ordered = midi_track.note_events.OrderBy(e => e.AbsoluteTime).ToList();

            List<NoteOnEvent>[] events_by_bars = new List<NoteOnEvent>[song.length_bars];
            {
                int x = 0;
                for (int i = 0; i < song.length_bars; ++i) {
                    for (; x < note_events_ordered.Count;) {
                        NoteOnEvent ev = note_events_ordered[x];
                        int bar = (int)(ev.AbsoluteTime / Variables.bar_ticks);

                        if (events_by_bars[i] == null) events_by_bars[i] = new List<NoteOnEvent>();

                        if (bar == i) {
                            events_by_bars[i].Add(ev);
                            ++x;
                        } else break;
                    }
                }
            }

            List<Song_Note>[] notes = new List<Song_Note>[moggsong.length + moggsong.countin];

            for (int i = 0; i < song.length_bars; ++i) {
                int event_count = events_by_bars[i].Count;
                for (int x = 0; x < event_count; ++x) {
                    NoteOnEvent ev = events_by_bars[i][x];

                    Song_Note note = new Song_Note() {
                        code = ev.NoteNumber,
                        lane = AMP_Constants.GetNoteLaneIndexFromCode(ev.NoteNumber),
                        track_id = track.id,
                        is_last_note = (x == event_count - 1),
                        ticks = ev.AbsoluteTime, bar = i,
                        bar_id = x
                    };
                    note.SetupTimeUnits(song.time_units); // TODO: move out here?

                    // TODO: If a measure does not have notes, the array entry for that list is going to be null.
                    // Should we allocate lists for empty bars as well? My gut says handle the null case.
                    if (notes[note.bar] == null) notes[note.bar] = new List<Song_Note>();
                    notes[note.bar].Add(note);
                }
            }

            Song_Section[] sections = new Song_Section[song.length_bars];
            for (int i = 0; i < song.length_bars; i++)
                sections[i] = new Song_Section() {
                    track_info = track,
                    id = i,
                    is_enabled = true,
                    is_empty = (notes[i] == null ? true : notes[i].Count == 0),
                    note_count = (notes[i] == null) ? 0 : notes[i].Count
                };

            track.notes = notes;
            track.sections = sections;
            track.audio_path = audio_path;

            tracks.Add(track);
        }

        // Set track info for Song:
        song.tracks = tracks;
        song.track_count = tracks.Count;

        return song;
    }
}