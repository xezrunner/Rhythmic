using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class Test : MonoBehaviour
{
    void Start()
    {
        AMP_MoggSong a = new AMP_MoggSong(@"G:\amp_ps3\songs\allthetime\allthetime.moggsong");
        Log("MoggSong CHECK: mogg_path: %  midi_path: %" , a.mogg_path, a.midi_path);

        AMP_MidiFile b = new AMP_MidiFile(@"G:\amp_ps3\songs\allthetime\allthetime.mid");
        Log("MidiFile CHECK: bpm: %", b.bpm);
    }
}
