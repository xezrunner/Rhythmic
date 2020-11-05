using System;
using System.Collections.Generic;
using System.Numerics;

public class MetaMeasure
{
    public int ID;
    public AmpTrack.InstrumentType Instrument;
    public bool IsCaptured;
    public bool IsBossMeasure; // shouldn't capture this measure when capturing a track from another measure
    public float StartDistance { get { return SongController.Instance.measureLengthInzPos * ID; } }
}

