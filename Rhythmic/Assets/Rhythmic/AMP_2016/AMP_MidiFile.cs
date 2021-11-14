using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Midi;
using static Logger;

// Fields:

public partial class AMP_MidiFile
{
    public int bpm;
    public AMP_MidiTrack[] tracks;
    public int track_count;
    public List<NoteOnEvent>[] note_on_events;
}

public struct AMP_MidiTrack
{
    public string _text;
    public int id;

    public AMP_Instrument instrument;
    public string name;
}

// Functionality:

public partial class AMP_MidiFile
{
    public AMP_MidiFile(string path) { ReadMIDIFromPath(path); }

    public void ReadMIDIFromPath(string path)
    {
        if (!File.Exists(path) && LogE("File does not exist: '%'".TM(this), path)) 
            return;

        byte[] bytes = File.ReadAllBytes(path);
        Stream stream = new MemoryStream(bytes);
        MidiFile midi = new MidiFile(stream, false); // TODO: Should 'strictChecking' be true?

        bpm = find_bpm_from_midi(midi);

        tracks = find_catch_tracks(midi);
        track_count = tracks.Length;

        note_on_events = find_note_events(midi);
    }

    // TODO: these functions could be static?

    int find_bpm_from_midi(MidiFile midi)
    {
        int event_count = midi.Events[0].Count;
        for (int i = 0; i < event_count; ++i)
        {
            MidiEvent ev = midi.Events[0][i];
            if (ev.GetType() != typeof(TempoEvent)) continue;

            TempoEvent tempo_ev = (TempoEvent)ev;
            int bpm = 60000 / (tempo_ev.MicrosecondsPerQuarterNote / 1000); // convert (us->ms) to minutes
            return bpm;
        }

        LogW("BPM not found!".TM(this));
        return 0;
    }

    List<NoteOnEvent>[] find_note_events(MidiFile midi)
    {
        return null;
    }

    const string MIDI_TRACKNAME_START = "0 SequenceTrackName T";
    const string MIDI_TRACKNAME_START_CUT = "0 SequenceTrackName TX ";
    AMP_MidiTrack[] find_catch_tracks(MidiFile midi)
    {
        List<AMP_MidiTrack> list = new List<AMP_MidiTrack>();
        int count = 0;
        for (int i = 0; i < midi.Tracks; ++i)
        {
            string code = midi.Events[i][0].ToString();
            if (!code.BeginsWith(MIDI_TRACKNAME_START)) continue;

            int cut_length = MIDI_TRACKNAME_START_CUT.Length;
            code = code.Substring(cut_length, code.Length - cut_length); // CATCH:TYPE:NAME
            // Log("[%]: %", i, code);

            string[] split = code.Split(':');

            // TODO: What should be done with freestyle tracks within the MIDI?
            // Currently, I'm handling them through the moggsong, but the game handles them through the MIDI...
            if (split[0] == "FREESTYLE") continue;

            AMP_MidiTrack t = new AMP_MidiTrack()
            {
                _text = code,
                id = count,
                instrument = AMP_Instrument.UNKNOWN,
                name = split[2]
            };
            AMP_Instrument instrument_parse;
            if (Enum.TryParse(split[1], true, out instrument_parse))
                t.instrument = instrument_parse;

            list.Add(t);
            ++count;
        }

        return list.ToArray();
    }
}