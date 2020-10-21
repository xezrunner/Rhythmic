using System;
using System.Collections.Generic;

public class MetaMeasure
{
    public int ID;
    public Track.InstrumentType Instrument;
    public bool IsCaptured;
    public bool IsBossMeasure; // shouldn't capture this measure when capturing a track from another measure
}

