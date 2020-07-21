using UnityEngine;
using System;
using System.Collections;

public class Catcher : MonoBehaviour
{
    public CatcherController CatcherController { get { return CatcherController.Instance; } }
    public enum CatchResult { Success = 0, Powerup = 1, Miss = 2, Empty = 3, Inactive = 4, Unknown = 5 }
    public enum NoteMissType { Mispress, Ignore, EmptyIgnore, EmptyMispress };

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
            else if (hitPoint.transform.name == "CATCHER")
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

                result.note = note;
                result.notetype = note.noteType;

                if (!note.IsNoteActive)
                {
                    result.catchresult = CatchResult.Inactive;
                    break;
                }

                CatchResult catchResult;
                if (note.noteType == Note.NoteType.Generic)
                    catchResult = CatchResult.Success;
                else
                    catchResult = CatchResult.Powerup;

                note.CaptureNote(); // destroy the note

                result.catchresult = catchResult;

                break;
            }
            else if (hitPoint.gameObject.tag == "MeasurePlane" & result.catchresult == null)
            {
                result.catchresult = CatchResult.Miss;
                result.noteMissType = NoteMissType.Mispress;

                break;
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
