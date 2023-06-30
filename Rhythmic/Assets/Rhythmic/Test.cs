using UnityEngine;
using static Logger;

public class Test : MonoBehaviour {
    void Start() {
        AMP_MoggSong a = new AMP_MoggSong(@"H:\HMXAMPLITUDE\Extractions\amplitude_ps4_extraction\ps4\songs\allthetime\allthetime.moggsong");
        Log("MoggSong CHECK: mogg_path: %  midi_path: %", a.mogg_path, a.midi_path);

        AMP_MidiFile b = new AMP_MidiFile(@"H:\HMXAMPLITUDE\Extractions\amplitude_ps4_extraction\ps4\songs\allthetime\allthetime.mid");
        Log("MidiFile CHECK: bpm: %", b.bpm);
        foreach (AMP_MidiTrack t in b.tracks) {
            Log("  - %:  id: %  instrument: %  name: %  |  note event count: %",
                t._text.AddColor(Colors.Application), t.id, t.instrument, t.name, t.note_events.Count);

            for (int i = 0; i < 10; ++i) {
                Log("    - [%] % (%) %", i, AMP_Constants.GetNoteLaneIndexFromCode(t.note_events[i].NoteNumber), t.note_events[i].NoteNumber,
                    ((AMP_NoteLane)t.note_events[i].NoteNumber).ToString().AddColor(Colors.Application));
            }
        }
    }
}
