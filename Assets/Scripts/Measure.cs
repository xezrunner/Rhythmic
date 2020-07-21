using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measure : MonoBehaviour
{
    public int measureNum; // measure ID
    public List<Note> noteList = new List<Note>(); // notes in this measure
    public List<MeasureSubBeat> subbeatList = new List<MeasureSubBeat>();
    public List<GameObject> subbeatObjectList = new List<GameObject>(); // TODO: this is bad

    public float startTimeInZPos;
    public float endTimeInZPos;

    /// <summary>
    /// If a measure doesn't contain any notes, this returns true.
    /// </summary>
    public bool IsMeasureEmpty { get { return noteList.Count == 0 ? true : false; } }

    /// <summary>
    /// If this is set to false, the measure GameObject will disable itself. This creates a hole in the tracks.
    /// </summary>
    bool _isMeasureActive = true;
    public bool IsMeasureActive
    {
        get { return _isMeasureActive; }
        set
        {
            _isMeasureActive = value;
            foreach (GameObject obj in subbeatObjectList)
            {
                obj.transform.GetChild(0).gameObject.SetActive(value);
            }
        }
    }

    bool _isMeasureCapturable = true;
    public bool IsMeasureCapturable // Is this measure capable of being captured? TODO: revisit this. Perhaps some corrupt measures? Lose streak when not capturable.
    {
        get { return _isMeasureCapturable; }
        set
        {
            _isMeasureCapturable = value;
            if (value)
                SetMeasureNotesActive(true);
            else
                SetMeasureNotesActive(false);
        }
    }

    public void SetMeasureNotesActive(bool state)
    {
        foreach (Note note in noteList)
            note.IsNoteActive = state;
    }
    public void SetMeasureNotesToBeCaptured(bool state = true)
    {
        if (!IsMeasureCapturable & !IsMeasureEmpty)
            return;

        foreach (Note note in noteList)
            note.IsNoteToBeCaptured = state;
    }

    public bool IsMeasureScorable = true; // Should this measure score points?
    public bool IsMeasureStreakable = true; // Should this measure count towards increasing the streak counter?

    // TODO: optimize!
    public MeasureSubBeat GetSubbeatForZpos(float zPos)
    {
        int counter = 0;
        foreach (MeasureSubBeat subbeat in subbeatList)
        {
            if (zPos < subbeat.EndZPos)
                return subbeat;
            /*
            else if (zPos == subbeat.EndZPos)
                return subbeatList[counter + 1];
            */
            counter++;
        }
        throw new Exception("Cannot find subbeat for this zPos: " + zPos);
    }
}
