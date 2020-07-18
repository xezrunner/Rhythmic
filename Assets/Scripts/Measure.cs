using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measure : MonoBehaviour
{
    public int measureNum; // measure ID
    public List<Note> noteList = new List<Note>(); // notes in this measure
    public List<GameObject> subbeatObjectList // TODO: this is bad
    {
        get
        {
            List<GameObject> finalList = new List<GameObject>();
            foreach (Transform subbeat in gameObject.transform)
            {
                if (subbeat.name.Contains("SUBBEAT"))
                    finalList.Add(subbeat.gameObject);
            }
            return finalList;
        }
    }

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
            gameObject.SetActive(false);
        }
    }

    public bool IsMeasureScorable = true; // Should this measure score points?
    public bool IsMeasureStreakable = true; // Should this measure count towards increasing the streak counter?
    public bool IsMeasureCapturable = true; // Is this measure capable of being captured? TODO: revisit this. Perhaps some corrupt measures? Lose streak when not capturable.
}
