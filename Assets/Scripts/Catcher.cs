using UnityEngine;
using System;
using System.Collections;

public class Catcher : MonoBehaviour
{
    public CatcherController CatcherController { get { return CatcherController.Instance; } }
    public enum CatchResult { Success = 0, Powerup = 1, Miss = 2, Empty = 3, Unknown = 4 }
    public enum NoteMissType { Ignore, Mispress, EmptyMispress };

    public Track.LaneType laneType;
    public float catchRadius = 0.27f;

    public event EventHandler<CatcherController.CatchEventArgs> OnCatch;

    public void PerformCatch()
    {
        var result = new CatcherController.CatchEventArgs();
        Collider[] cast = Physics.OverlapSphere(transform.position, catchRadius); // Perform a raycast downwards from the catcher

        foreach (Collider hitPoint in cast)
        {
            if (hitPoint.transform == transform)
                continue;
            /*
            if (hitPoint.gameObject.GetComponent<Note>() == null) // if the GameObject we hit doesn't have a Note (CATCH) component
            {
                result.catchresult = CatchResult.Unknown; // unknown object
                break;
            }
            */
            else if (hitPoint.gameObject.GetComponent<Note>() != null) // if we have a Note (CATCH)
            {
                // get the Note (CATCH) object from the hit object
                Note note = hitPoint.gameObject.GetComponent<Note>();

                CatchResult catchResult;
                if (note.noteType == Note.NoteType.Generic)
                    catchResult = CatchResult.Success;
                else
                    catchResult = CatchResult.Powerup;

                note.CaptureNote(); // destroy the note

                result.catchresult = catchResult;
                result.note = note;
                result.notetype = note.noteType;

                break;
            }
            else if (hitPoint.gameObject.tag == "MeasurePlane")
            {
                result.catchresult = CatchResult.Miss;
                result.noteMissType = NoteMissType.Mispress;
            }
        }

        // if we haven't set a result, that means that the result is Empty
        if (result.catchresult == null)
            result.catchresult = CatchResult.Empty;

        OnCatch?.Invoke(null, result);
    }

    private void OnDrawGizmos()
    {
        if (!RhythmicGame.DebugCatcherCasting)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchRadius);
    }
}
