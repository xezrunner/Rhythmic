using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class Test : MonoBehaviour
{
    void Start()
    {
        AMP_MoggSong a = new AMP_MoggSong(@"G:\amp_ps3\songs\allthetime\allthetime.moggsong");
        Log("mogg_path: %  midi_path: %" , a.mogg_path, a.midi_path);
    }
}
