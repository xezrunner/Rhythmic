using UnityEngine;
using System;
using System.Collections;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

public class Catcher : MonoBehaviour
{
    public CatcherController CatcherController { get { return CatcherController.Instance; } }
    public enum CatchResult { Success = 0, Powerup = 1, Miss = 2, Empty = 3, Inactive = 4, Unknown = 5 }
    public enum NoteMissType { Mispress, Ignore, EmptyIgnore, EmptyMispress };

    public Track.LaneType laneType;
    public float catchRadiusExtra = 0.6f;
    public float catchRadiusZOffset = 3.5f;

    public event EventHandler<CatcherController.CatchEventArgs> OnCatch;
    public void PerformCatch()
    {
        var result = new CatcherController.CatchEventArgs();

        Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 radius = new Vector3(transform.localScale.x, transform.localScale.y, (transform.localScale.z + catchRadiusExtra));

        Collider[] cast = Physics.OverlapBox(pos, radius); // Perform a raycast downwards from the catcher
        List<Note> noteList = new List<Note>();
        bool foundMeasurePlane = false;

        foreach (Collider hitPoint in cast) // go through every found object from the raycast
        {
            if (hitPoint.tag == "Note") // if we hit a Note (CATCH)
            {
                Note note = hitPoint.gameObject.GetComponent<Note>();
                noteList.Add(note); // add Note to the note list
            }
            else if (hitPoint.name == "MeasurePlane") // we hit the track's plane
            {
                // in case we find notes after the measure plane, we want to cancel the measure plane registration
                foundMeasurePlane = true;
            }
        }

        if (foundMeasurePlane & noteList.Count == 0) // if we hit the measure plane and there are no notes
        {
            result.catchresult = CatchResult.Miss;
            result.noteMissType = NoteMissType.Mispress;
        }
        else if (noteList.Count != 0) // if there are notes
        {
            // if we have multiple notes in a single hit, sort the notes based on their zPos, so that we know which one is the first one.
            if (noteList.Count > 1)
            {
                List<Note> newList = noteList.OrderBy(o => o.zPos).ToList();
                noteList = newList;
            }

            // We only want to catch the first note when we have multiple.
            // The rest of the notes should be caught with following catches.
            Note note = noteList[0];

            result.note = note;
            result.notetype = note.noteType;

            if (!note.IsNoteEnabled) // if the note is inactive, we want to play the note animation, but do nothing else
                result.catchresult = CatchResult.Inactive;
            else
            {
                CatchResult catchResult;
                if (note.noteType == Note.NoteType.Generic)
                    catchResult = CatchResult.Success;
                else
                    catchResult = CatchResult.Powerup;

                note.CaptureNote(); // capture the note

                result.catchresult = catchResult;
            }

        }

        // if we haven't set a result, deafult to Empty
        if (result.catchresult == null)
            result.catchresult = CatchResult.Empty;

        OnCatch?.Invoke(null, result);
    }

    // DEBUG DRAWING

    private void OnDrawGizmos()
    {
        if (!RhythmicGame.DebugCatcherCasting)
            return;

        Gizmos.color = Color.red;
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 radius = new Vector3(transform.localScale.x, transform.localScale.y, (transform.localScale.z + catchRadiusExtra));
        Gizmos.DrawWireCube(pos, radius);
    }
}
