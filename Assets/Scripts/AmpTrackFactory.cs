using UnityEngine;

/// <summary>
/// Factory class for generating measures and notes in an AmpTrack
/// </summary>

public partial class AmpTrack
{
    [Header("Prefabs")]
    public GameObject TrackSectionPrefab;
    public GameObject NotePrefab;

    /// <summary>
    /// Creates a measure and adds it to the measure list.
    /// </summary>
    public AmpTrackSection CreateMeasure(MetaMeasure meta, AmpTrack track)
    {
        /// Create object
        var obj = Instantiate(TrackSectionPrefab);
        obj.transform.parent = MeasureContainer;

        /// Get and configure script
        AmpTrackSection measure = obj.GetComponent<AmpTrackSection>();
        measure.Track = track;
        measure.ID = meta.ID;
        measure.Instrument = Instrument;
        measure.Length = SongController.measureLengthInzPos;
        measure.Color = Color;

        // TODO: possibly simplify position &/ rotation properties?
        Vector3 measurePos = new Vector3(
            RhythmicGame.TrackWidth * RealID, // X is the track's horizontal position
            0, meta.StartDistance); // Z is the distance at which the measure needs to be placed

        measure.PositionOnPath = measurePos;
        measure.PositionOnPath.z = measurePos.z;
        measure.RotationOnPath = zRot;

        /// Add measure to measure list
        Measures.Add(measure);
        return measure;
    }

    public AmpNote CreateNote(MetaNote meta, AmpTrackSection measure = null)
    {
        /// Create object
        var obj = Instantiate(NotePrefab);
        if (measure) obj.transform.parent = measure.NoteContainer;

        obj.transform.position = Path.GetPointAtDistance(meta.Distance);
        obj.transform.rotation = Path.GetRotationAtDistance(meta.Distance) * Quaternion.Euler(0, 0, 90);
        obj.transform.Translate(Vector3.right * (GetLocalXPosFromLaneType(meta.Lane) + (RhythmicGame.TrackWidth * RealID)));

        // set up
        obj.name = meta.Name;
        AmpNote note = obj.GetComponent<AmpNote>();
        note.TrackID = ID;
        note.MeasureID = meta.MeasureID;
        note.Lane = meta.Lane;
        note.Distance = meta.Distance;

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
    public Note.NoteType Type;
    public AmpTrack.LaneSide Lane;
    public int MeasureID;
    public float Distance;
}