using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

// Factory class for generating measures and notes in an AmpTrack

public partial class AmpTrack
{
    [Header("Prefabs")]
    public GameObject TrackSectionPrefab;
    public GameObject NotePrefab;

    /// <summary>
    /// Creates a measure and adds it to the measure list.
    /// </summary>
    public AmpTrackSection CreateMeasure(MetaMeasure meta)
    {
        /// Create object
        var obj = Instantiate(TrackSectionPrefab);
        obj.transform.parent = MeasureContainer;

        /// Get and configure script
        AmpTrackSection measure = obj.GetComponent<AmpTrackSection>();
        measure.Track = this;
        measure.ID = meta.ID;
        measure.Instrument = Instrument;
        measure.Length = SongController.measureLengthInzPos;
        measure.EdgeLightsColor = Color;
        measure.IsCaptured = meta.IsCaptured;

        // TODO: possibly simplify position &/ rotation properties?
        Vector3 measurePos = new Vector3(
            TunnelPos.x, // X is the track's horizontal position
            TunnelPos.y, meta.StartDistance); // Z is the distance at which the measure needs to be placed

        measure.Position = measurePos;
        measure.Rotation = TunnelRot.z;

        /// Add measure to measure list
        Measures.Add(measure);
        return measure;
    }

    public AmpNote CreateNote(MetaNote meta, AmpTrackSection measure = null, bool lastNote = false)
    {
        /// Create object
        var obj = Instantiate(NotePrefab);
        if (measure) obj.transform.parent = measure.NotesContainer;

        Vector3 offset = TunnelPos - Tunnel.center;
        obj.transform.position = PathTools.GetPositionOnPath(Path, meta.Distance, offset);
        obj.transform.rotation = PathTools.GetRotationOnPath(Path, meta.Distance, TunnelRot);

        Vector3 localRight = obj.transform.right; // After rotating the object on the path, we have the normal direction to the right
        Vector3 laneOffset = localRight * GetLocalXPosFromLaneType(meta.Lane);

        obj.transform.Translate(laneOffset, Space.World); // Translate to lane

        // set up
        obj.name = meta.Name;
        AmpNote note = obj.GetComponent<AmpNote>();
        note.Track = this;
        note.TrackID = RealID;
        note.MeasureID = meta.MeasureID;
        note.Lane = meta.Lane;
        note.Distance = meta.Distance;
        note.IsCaptured = meta.IsCaptured;
        note.IsLastNote = lastNote;

        measure.Notes.Add(note);
        return note;
    }
}

public class MetaMeasure
{
    public int ID;
    public AmpTrack.InstrumentType Instrument;
    public bool IsCaptured;
    public bool IsBossMeasure; // shouldn't capture this measure when capturing a track from another measure
    //public float StartDistance { get { return (GameObject.Find("Path").GetComponent<PathCreation.PathCreator>().path.length - SongController.Instance.songLengthInMeasures * SongController.Instance.measureLengthInzPos) + SongController.Instance.measureLengthInzPos * ID; } }
    public float StartDistance { get { return SongController.Instance.measureLengthInzPos * ID; } }
}

public class MetaNote
{
    public string Name;
    public NoteType Type;
    public LaneSide Lane;
    public int MeasureID;
    public float Distance;
    public bool IsCaptured;
}