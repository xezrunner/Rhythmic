using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Midi;

public class AMPMidi_Info
{
    public int bpm;
    public int ticks;

    public string song_name;

    //public List<string> midi_tracks = new List<string>();
    public int track_count;
    public List<string> song_tracks;
    public List<string> midi_tracks;
    public List<NoteOnEvent>[] song_note_events;
}

// Partially based on https://gist.github.com/CustomPhase/033829c5c30f872d250a79e3d35b7048
public static class AmplitudeMidiReader
{
    static int ticks_target = 480;

    static string get_midi_path(string song_name)
    {
        return AmplitudeGame.AMP_GetSongFilePath(song_name, AmplitudeGame.AMP_FileExtension.mid);
    }

    public static AMPMidi_Info LoadMIDI(string song_name)
    {
        string file_path = get_midi_path(song_name);
        if (!File.Exists(file_path)) return null;

        byte[] byte_array = File.ReadAllBytes(file_path);
        Stream stream = new MemoryStream(byte_array);
        MidiFile midi = new MidiFile(stream, true);

        AMPMidi_Info midi_info = new AMPMidi_Info()
        {
            song_name = song_name,
            ticks = midi.DeltaTicksPerQuarterNote, // NOTE: should be 480 at all times - conversions might be needed!
            bpm = get_midi_bpm(midi),
            song_tracks = get_song_tracks(midi),
            midi_tracks = get_song_tracks(midi, full_code: true),
        };
        midi_info.track_count = midi_info.song_tracks.Count;
        midi_info.song_note_events = get_song_note_events(midi, midi_info);

        return midi_info;
    }

    static int get_midi_bpm(MidiFile midi)
    {
        int event_count = midi.Events[0].Count;
        for (int i = 0; i < event_count; ++i)
        {
            MidiEvent midi_event = midi.Events[0][i];
            // Find tempo event:
            if (midi_event.GetType() != typeof(TempoEvent)) continue;

            TempoEvent tempo_event = (TempoEvent)midi_event;
            int bpm = 60000 / (tempo_event.MicrosecondsPerQuarterNote / 1000);
            return bpm;
        }

        Logger.LogError("Could not find BPM! This is bad!".T("AmpllitudeMidiReader"));
        return 0;
    }

    static List<NoteOnEvent>[] get_song_note_events(MidiFile midi, AMPMidi_Info info)
    {
        List<NoteOnEvent>[] list_array = new List<NoteOnEvent>[info.track_count];
        for (int i = 0; i < info.track_count; ++i) list_array[i] = new List<NoteOnEvent>();

        for (int i = 0; i < info.track_count; ++i)
        {
            // HACK: first track is always the song name (meta) - check name of track instead!
            foreach (MidiEvent midi_event in midi.Events[i + 1])
            {
                if (midi_event.CommandCode == MidiCommandCode.NoteOn)
                {
                    NoteOnEvent note_on = (NoteOnEvent)midi_event;

                    // HACK: ticks correction (copy-pasted) | TODO: improve/revise!
                    if (info.ticks != ticks_target)
                    {
                        // If the MIDI has a different tick unit as opposed to the standard 480 (ticks_target), then
                        // note distance calculations will be wrong (spaced out by a huge amount).
                        // The MIDI library we are using calculates the time by its own, we can't control that.
                        // We can, however, correct it after the fact by getting the 'tick-less' time and calculating + setting it with 480 ticks.
                        // -xezrunner, 25.04.2021
                        long abs_time = note_on.AbsoluteTime;
                        double abs_nonticks = (double)abs_time / (info.bpm * info.ticks);
                        note_on.AbsoluteTime = (long)(abs_nonticks * (info.bpm * ticks_target));
                    }

                    if (AmplitudeGame.CurrentNoteNumberSet.Any(n => (n == note_on.NoteNumber)))
                        list_array[i].Add(note_on);
                }
            }
        }

        return list_array;
    }

    static List<string> get_song_tracks(MidiFile midi, bool full_code = false)
    {
        List<string> list = new List<string>();

        for (int i = 0; i < midi.Tracks; ++i)
        {
            // "0 SequenceTrackName T1 CATCH:D:Drums1"

            // "T1 CATCH:D:Drums1"
            string code = //full_code ? midi.Events[i][0].ToString() :
                midi.Events[i][0].ToString().Substring("0 SequenceTrackName ".Length);

            if (full_code)
            {
                string[] tokens = code.Split(':');
                if (!tokens[0].Contains("CATCH") && tokens[0] != "BG_CLICK") continue;
                code = string.Join("_", tokens);
            }

            if (code.Contains("CATCH"))
                list.Add(code);
            //else if (code.Contains("FREESTYLE"))
            //    list.Add("freestyle");
            else if (code.Contains("BG_CLICK"))
               list.Add("bg_click");
        }

        return list;
    }
}