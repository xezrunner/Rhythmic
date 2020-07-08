using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackLane : MonoBehaviour
{
    public enum LaneType { Left = 0, Center = 1, Right = 2 } // lane side
    public List<Note> LaneNotes = new List<Note>(); // notes in this lane

    public LaneType laneType;

    /// <summary>
    /// Creates a note (CATCH) object on the track.
    /// </summary>
    /// <param name="zPosition">The Z axis offset of the note (distance)</param>
    /// <param name="noteName">The name of the note object</param>
    public void CreateNoteObject(float zPosition, string noteName = "", Note.NoteType noteType = Note.NoteType.Generic, LaneType noteLane = LaneType.Center)
    {
        // create GameObject for Note
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.transform.parent = null;
        obj.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        obj.transform.SetParent(gameObject.transform, true);
        obj.transform.position = new Vector3(gameObject.transform.position.x, 0.01f, zPosition);
        obj.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/NoteMaterial");
        obj.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        obj.AddComponent<BoxCollider>();

        // create and assign Note to GameObject
        Note note = obj.AddComponent<Note>();
        note.name = noteName + "_" + obj.transform.position.z;
        note.noteType = noteType;
        note.noteLane = noteLane;

        // Add note to Notes list
        LaneNotes.Add(note);
    }

    /// <summary>
    /// Returns the X position for the specified lane inside the local GameObject
    /// </summary>
    /// <param name="laneType">The lane</param>
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
