using UnityEngine;
using System.Collections;
using System;
using System.Dynamic;

public class Note : MonoBehaviour
{   
    //public string noteName { get { return gameObject.name; } set { gameObject.name = value; } }
    public NoteType noteType;
    public Track noteTrack;
    public Track.LaneType noteLane;
    public TrackMeasure noteMeasure;
    public int measureNum;

    /// <summary>
    /// This event is invoked when the note is blasted.
    /// </summary>
    //public event EventHandler<NoteType> OnCatch;

    public enum NoteType
    {
        Generic = 0, // a generic note
        Autoblaster = 1, // Cleanse
        Slowdown = 2, // Sedate
        Multiply = 3, // Multiply
        Freestyle = 4, // Flow
        Autopilot = 5, // Temporarily let the game play itself
        STORY_Corrupt = 6, // Avoid corrupted nanotech!
        STORY_Memory = 7 // Temporarily shows memories as per the lore
    }
}