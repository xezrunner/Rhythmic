using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class Catcher : MonoBehaviour
{
    /// <summary>
    /// TODO: KeyCodes based on user input settings
    /// </summary>

    PlayerController player { get { return GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>(); } }

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
    public enum NoteMissType { Ignore, Mispress, EmptyMispress };

    public TrackLane.LaneType laneType;

    public Note IgnoreNote { get; set; }
    Note ShouldHit;
    private void Update()
    {
        // Perform a raycast downwards from the catcher
        Collider[] cast = Physics.OverlapSphere(transform.position, 0.35f);
        bool foundNote = false; // currently found note

        foreach (Collider hitPoint in cast) // if we hit
        {
            if (hitPoint.transform == transform)
                continue;

            if (hitPoint.gameObject.GetComponents<Note>().Length > 0) // if the GameObject we hit has a Note (CATCH) component
            {
                if (hitPoint.gameObject.GetComponent<Note>() == IgnoreNote) // if it's the note that should be ignored, do nothing
                    break;
                ShouldHit = hitPoint.gameObject.GetComponent<Note>();
                player.ExpectedNote = ShouldHit;
                foundNote = true;
            }
        }

        if (ShouldHit != null & !foundNote)
        {
            player.DeclareNoteMiss(ShouldHit, NoteMissType.Mispress);
            ShouldHit = null;
        }
    }

    public event EventHandler<CatchEventResult> OnCatch;

    public class CatchEventResult
    {
        public CatchResult? catchresult;
        public Note note;
        public Note.NoteType? notetype;
    }

    public void PerformCatch(KeyCode pressedKey)
    {
        CatchEventResult result = new CatchEventResult();
        Collider[] cast = Physics.OverlapSphere(transform.position, 0.35f); // Perform a raycast downwards from the catcher

        foreach (Collider hitPoint in cast)
        {
            if (hitPoint.transform == transform)
                continue;
            if (hitPoint.gameObject.GetComponent<Note>() == null) // if the GameObject we hit doesn't have a Note (CATCH) component
            {
                result.catchresult = CatchResult.Unknown; // unknown object
                break;
            }
            else // if we have a Note (CATCH)
            {
                // get the Note (CATCH) object from the hit object
                Note note = hitPoint.gameObject.GetComponent<Note>();

                if (pressedKey == LaneToKeyCode(note.noteLane)) // if the pressed input was the correct key for this catcher
                {
                    CatchResult catchResult;
                    if (note.noteType != Note.NoteType.Generic)
                        catchResult = CatchResult.Powerup;
                    else
                        catchResult = CatchResult.Success;

                    Destroy(hitPoint.gameObject); // destroy the note

                    result.catchresult = catchResult;
                    result.note = note;
                    result.notetype = note.noteType;

                    ShouldHit = null;
                    player.ExpectedNote = null;
                    player.shouldReactToNoteMiss = true;

                    break;
                }
                else // invalid key
                {
                    result.catchresult = CatchResult.Failure;
                    result.note = note;
                }
            }
        }

        // if we haven't set a result, that means that the result is Empty
        if (result.catchresult == null)
        {
            result.catchresult = CatchResult.Empty;
            result.note = ShouldHit;
        }

        OnCatch?.Invoke(null, result);
    }

    public void OnDrawGizmosSelected()
    {
        if (!RhythmicGame.DebugCatcherCasting)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }

    /// <summary>
    /// Gives back the appropriate key for the lane type.
    /// </summary>
    /// <param name="lane">The lane in question</param>
    public static KeyCode LaneToKeyCode(TrackLane.LaneType lane)
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
    public static TrackLane.LaneType KeyCodeToLane(KeyCode key)
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
