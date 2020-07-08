using UnityEngine;
using System.Collections;
using System;

public class Catcher : MonoBehaviour
{
    /// <summary>
    /// TODO: KeyCodes based on user input settings
    /// </summary>

    public KeyCode[] keycodes = new KeyCode[] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow };
    public KeyCode[] keycodes_left = new KeyCode[] { KeyCode.LeftArrow, };
    public KeyCode[] keycodes_center = new KeyCode[] { KeyCode.UpArrow, };
    public KeyCode[] keycodes_right = new KeyCode[] { KeyCode.RightArrow };

    /// <summary>
    /// Failure means the wrong input was pressed.
    /// Empty means that there was no note.
    /// Unknown is an unknown GameObject with no Note (CATCH) object on it.
    /// </summary>
    public enum CatchResult { Success = 0, Powerup = 1, Failure = 2, Empty = 3, Unknown = 4 }

    public TrackLane.LaneType laneType;

    private void Update()
    {

    }

    public event EventHandler<Note.NoteType> OnCatch;

    /*
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
    */

    public CatchResult PerformCatch(KeyCode pressedKey)
    {
        // Perform a raycast downwards from the catcher
        foreach (Collider hitPoint in Physics.OverlapSphere(transform.position, 0.35f)) // if we hit
        {
            if (hitPoint.gameObject.GetComponents<Note>().Length < 1) // if the GameObject we hit doesn't have a Note (CATCH) component
                return CatchResult.Unknown; // unknown object

            // get the Note (CATCH) object from the hit object
            Note note = hitPoint.gameObject.GetComponent<Note>();

            // if the pressed input was the correct key for this catcher
            if (pressedKey == LaneToKeyCode(note.noteLane))
            {
                Destroy(hitPoint.gameObject); // destroy the note
                OnCatch?.Invoke(null, note.noteType);

                if (note.noteType != Note.NoteType.Generic)
                    return CatchResult.Powerup;
                else
                    return CatchResult.Success; // return TRUE for successful catch!
            }
            else
                return CatchResult.Failure; // invalid key
        }
        return CatchResult.Empty; // nothing
    }

    /// <summary>
    /// Gives back the appropriate key for the lane type.
    /// </summary>
    /// <param name="lane">The lane in question</param>
    KeyCode LaneToKeyCode(TrackLane.LaneType lane)
    {
        switch (lane)
        {
            case TrackLane.LaneType.Left:
                return KeyCode.LeftArrow;
            case TrackLane.LaneType.Center:
                return KeyCode.UpArrow;
            case TrackLane.LaneType.Right:
                return KeyCode.RightArrow;

            default:
                return KeyCode.None;
        }
    }
    /// <summary>
    /// Gives back the appropriate track for the input key
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    TrackLane.LaneType KeyCodeToLane(KeyCode key)
    {
        switch (key)
        {
            default:
                return TrackLane.LaneType.Center;

            case KeyCode.LeftArrow:
                return TrackLane.LaneType.Left;
            case KeyCode.UpArrow:
                return TrackLane.LaneType.Center;
            case KeyCode.RightArrow:
                return TrackLane.LaneType.Right;
        }
    }
}
