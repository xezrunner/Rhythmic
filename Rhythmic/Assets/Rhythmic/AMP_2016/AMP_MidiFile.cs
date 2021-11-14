using System.IO;
using NAudio.Midi;
using static Logger;

// Fields:

public partial class AMP_MidiFile
{
    public int bpm;
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
    }

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
}