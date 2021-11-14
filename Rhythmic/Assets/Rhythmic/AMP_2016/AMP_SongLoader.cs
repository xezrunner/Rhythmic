using System.Collections.Generic;
using System.IO;
using NAudio.Midi;
using static Logger;

public static class AMP_SongLoader
{
    public static Song LoadSong(string song_name)
    {
        // TODO: Revise and error checking!!!
        string moggsong_path = Path.Combine(AMP_Constants.MOGGSONG_PATH, song_name, song_name + ".moggsong");
        if (!File.Exists(moggsong_path) && LogE("The .moggsong for song % doesn't exist at '%'.".TM(nameof(AMP_SongLoader)),
            song_name, moggsong_path)) return null;
        AMP_MoggSong moggsong = new AMP_MoggSong(moggsong_path);

        string midi_path = Path.Combine(AMP_Constants.MIDI_PATH, song_name, song_name + ".mid");
        if (!File.Exists(midi_path) && LogE("The .mid for song % doesn't exist at '%'.".TM(nameof(AMP_SongLoader)),
            song_name, midi_path)) return null;
        AMP_MidiFile midifile = new AMP_MidiFile(midi_path);

        // Build tracks:
        List<Song_Track> tracks = new List<Song_Track>();
        foreach (AMP_MidiTrack midi_track in midifile.tracks)
        {
            string track_name_for_audio = midi_track._text.Replace(':', '_');
            string audio_path = Path.Combine(AMP_Constants.AUDIO_PATH, song_name, "audio", track_name_for_audio + ".ogg");

            if (!File.Exists(audio_path))
                LogW("The audio for song % doesn't exist at '%'.".TM(nameof(AMP_SongLoader)), song_name, audio_path);

            // TODO: refer to Song.cs/Song_Instrument!
            Song_Instrument instrument = Song_Instrument.UNKNOWN;
            instrument = (Song_Instrument)midi_track.instrument;

            List<Song_Note> notes = new List<Song_Note>();
            foreach (NoteOnEvent ev in midi_track.note_events)
            {
                Song_Note note = new Song_Note()
                {
                    pos_ticks = ev.AbsoluteTime,
                    lane = AMP_Constants.GetNoteLaneIndexFromCode(ev.NoteNumber)
                };
                notes.Add(note);
            }

            Song_Track track = new Song_Track()
            {
                id = midi_track.id,
                name = midi_track.name,
                instrument = instrument,
                notes = notes,
                audio_path = audio_path
            };

            tracks.Add(track);
        }

        Song song = new Song()
        {
            name = song_name,
            path = Path.Combine(AMP_Constants.MOGGSONG_PATH, song_name),

            length_bars = moggsong.length,
            countin = moggsong.countin,

            tracks = tracks,
            track_count = tracks.Count,

            //world_name = moggsong.arena_path,
            tunnel_scale = moggsong.tunnel_scale,

            enable_order = moggsong.enable_order,
            section_bars = moggsong.section_start_bars,
            score_goals = moggsong.score_goal
        };

        return song;
    }
}