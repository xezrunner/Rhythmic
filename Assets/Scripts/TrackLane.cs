using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackLane : MonoBehaviour
{
    public enum LaneType { Left = 0, Center = 1, Right = 2 } // lane side

    public string trackName { get; set; }
    public LaneType laneType;
    public List<Note> LaneNotes = new List<Note>(); // notes in this lane

    /// <summary>
    /// Creates a note (CATCH) object on the track.
    /// </summary>
    /// <param name="zPosition">The Z axis offset of the note (distance)</param>
    /// <param name="noteName">The name of the note object</param>
    public void CreateNoteObject(float zPosition, string noteName = "", Note.NoteType noteType = Note.NoteType.Generic, LaneType noteLane = LaneType.Center)
    {
        // create GameObject for Note
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.parent = gameObject.transform;
        obj.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, zPosition);
        obj.AddComponent<BoxCollider>();

        // create and assign Note to GameObject
        Note note = obj.AddComponent<Note>();
        note.name = noteName;
        note.noteType = noteType;
        note.noteLane = noteLane;

        LaneNotes.Add(note);
    }

    public static float GetLocalXPosFromLaneType(LaneType laneType)
    {
        switch (laneType)
        {
            default:
                return 0f;

            case LaneType.Left:
                return -1f;
            case LaneType.Center:
                return 0f;
            case LaneType.Right:
                return 1f;
        }
    }
}
