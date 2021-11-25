using NAudio.Midi;
using System.Collections.Generic;
using System.IO;
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
            instrument = (InstrumentType)midi_track.instrument;

            List<Song_Note>[] notes = new List<Song_Note>[moggsong.length + moggsong.countin];
            foreach (NoteOnEvent ev in midi_track.note_events) {
                Song_Note note = new Song_Note(ev.NoteNumber,
                                               AMP_Constants.GetNoteLaneIndexFromCode(ev.NoteNumber),
                                               ev.AbsoluteTime,
                                               song.time_units);

                // TODO: If a measure does not have notes, the array entry for that list is going to be null.
                // Should we allocate lists for empty bars as well? My gut says handle the null case.
                if (notes[note.bar] == null) notes[note.bar] = new List<Song_Note>();
                notes[note.bar].Add(note);
            }

            Song_Track track = new Song_Track() {
                id = midi_track.id,
                name = midi_track.name,
                instrument = new Song_Instrument(instrument),
                notes = notes,
                audio_path = audio_path
            };

            tracks.Add(track);
        }

        // Set track info for Song:
        song.tracks = tracks;
        song.track_count = tracks.Count;

        return song;
    }
}