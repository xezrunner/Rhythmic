using System;
using System.Collections.Generic;
using UnityEngine;

public partial class SongController
{
    [NonSerialized] public float songFudgeFactor = 1f; // Default is 1
    public float totalFudgeFactor = 1f; // Default is 1

    /// Time & unit calculations
    /*---- SONG UNITS ------
    Beat (Quarter note): One beat's length in ticks | 1 beat = 480 ticks
    Measure (Bar): 4 beats' length in ticks | 1 measure = 480 * 4 ticks (1920 ticks)
    ----- GENERIC UNITS -----
    s (Seconds)
    ms (Milliseconds)
    zPos (Meters)
    */

    // Common 'meta' units
    public float beatPerSec { get { return songBpm / 60f; } }
    public float secPerBeat { get { return 60f / songBpm; } }

    [NonSerialized] public int beatTicks = 480;
    public int measureTicks { get { return beatTicks * 4; } }
    public int subbeatTicks { get { return beatTicks / 2; } }

    public float measureLengthInzPos { get { return TickToPos(measureTicks); } }
    public float subbeatLengthInzPos { get { return TickToPos(subbeatTicks); } }

    /// Individual time & position units
    // Ticks - how many ticks in a <unit>
    [NonSerialized] public float tickInSec;
    [NonSerialized] public float tickInMs;
    [NonSerialized] public float tickInPos;

    // Seconds - how many seconds in a <unit>
    [NonSerialized] public float secInTick;
    [NonSerialized] public float secInMs;
    [NonSerialized] public float secInPos;

    // Milliseconds (ms) - how many ms in a <unit>
    [NonSerialized] public float msInTick;
    [NonSerialized] public float msInSec;
    [NonSerialized] public float msInPos;

    // Meters (pos) - how many pos in a <unit>
    [NonSerialized] public float posInTick;
    [NonSerialized] public float posInSec;
    [NonSerialized] public float posInMs;

    /// Convertors
    // Ticks -> 
    public float TickToSec(float ticks) { return secInTick * ticks; }
    public float TickToMs(float ticks) { return msInTick * ticks; }
    public float TickToPos(float ticks) { return posInTick * ticks; }

    // Seconds ->
    public float SecToTick(float sec) { return tickInSec * sec; }
    public float SecToMs(float sec) { return msInSec * sec; }
    public float SecToPos(float sec) { return posInSec * sec; }

    // Milliseconds
    public float MsToTick(float ms) { return tickInMs * ms; }
    public float MsToSec(float ms) { return secInMs * ms; }
    public float MsToPos(float ms) { return posInMs * ms; }

    // Position (meters)
    public float PosToTick(float pos) { return tickInPos * pos; }
    public float PosToSec(float pos, bool scale = false)
    {
        float value = secInPos * pos;
        value += (value * totalFudgeFactor);
        //if (scale) value *= RhythmicGame.DifficultyFudgeFactor / songFudgeFactor;
        //else value /= songFudgeFactor * RhythmicGame.DifficultyFudgeFactor;

        return value;
    }
    public float PosToMs(float pos, bool scale = false)
    {
        float value = msInPos * pos;
        value += (value * totalFudgeFactor);
        //if (scale) value *= RhythmicGame.DifficultyFudgeFactor / songFudgeFactor;
        //else value /= songFudgeFactor * RhythmicGame.DifficultyFudgeFactor;

        return value;
    }

    /// Miscellaneous converters
    public float MeasureToPos(int measureID) { return StartDistance + (measureID * measureLengthInzPos); }

    // zPos to measure/subbeat num conversion
    // TODO: revise
    // Gets the measure number for a z position (Rhythmic Game unit)
    public int GetMeasureNumForZPos(float zPos)
    {
        /*
        zPos = (float)Math.Round(zPos, 1);
        foreach (MeasureInfo measure in songMeasures)
        {
            float endTimeInZPos = (float)Math.Round(measure.endTimeInzPos, 1);
            if (zPos < endTimeInZPos)
                return measure.measureNum;
            else
                continue;
        }
        */
        for (int i = 0; i < songLengthInMeasures; i++)
            if (Math.Round(zPos, 1) < (i + 1) * Math.Round(measureLengthInzPos, 1)) // TODO: might not need rounding?
                return i;

        return -1;
    }
    public int GetSubbeatNumForZPos(int measureNum, float zPos)
    {
        /*
        zPos = (float)Math.Round(zPos, 1);

        MeasureInfo info = songMeasures[measureNum];
        float measureTime = (float)Math.Round((info.startTimeInzPos + subbeatLengthInzPos), 1);

        int finalValue = -1;

        for (int i = 0; i < 8; i++)
        {
            if (zPos < measureTime)
                return i;
            else
                measureTime += subbeatLengthInzPos;
        }

        Debug.LogErrorFormat("AMP_CTRL: couldn't find subbeat for note at measure {0}, zPos {1}", measureNum, zPos);
        return finalValue;
        */
        float position = (float)Math.Round((measureNum + 1) * measureLengthInzPos);

        for (int i = 0; i < 8; i++)
        {
            if (Math.Round(zPos, 1) < position) // TODO: might not need rounding?
                return i;
            else
                position += subbeatLengthInzPos;
        }

        return -1;
    }

    public void CalculateTimeUnits()
    {
        // TODO: We should prefer one over the other, probably. Doesn't look like the OG game mixes it.
        totalFudgeFactor = (/*songFudgeFactor + */RhythmicGame.DifficultyFudgeFactor);

        // Seconds
        secInTick = 60f / (songBpm * beatTicks);
        secInMs = 0.001f; // 1s = 1000ms
        secInPos = (secPerBeat / 4); // * songFudgeFactor / RhythmicGame.DifficultyFudgeFactor

        // Milliseconds
        msInTick = secInTick * 1000;
        msInSec = 1000; // 1ms = 0.001s
        msInPos = secInPos * 1000;

        // Position (meters)
        posInSec = (4 / secPerBeat) + ((4 / secPerBeat) * totalFudgeFactor);
        posInMs = (4 / secPerBeat / 1000) + ((4 / secPerBeat / 1000) * totalFudgeFactor);
        posInTick = posInSec * secInTick;

        // Ticks
        tickInSec = (songBpm * beatTicks) / 60; // how many ticks in a second // 880
        tickInMs = tickInSec / 1000; // How many ticks in a milliseconds // ???
        tickInPos = secInPos * tickInSec;

        // Find starting position point
        // This adjusts the positioning of stuff in a way that we always reach the very end of the path
        if (StartDistanceAdjustmentEnabled)
            StartDistance = PathTools.Path.length - (songLengthInMeasures * measureLengthInzPos);
    }
}