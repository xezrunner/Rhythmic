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
    public AmpTrackSection CreateMeasure(MetaMeasure meta)
    {
        /// Create object
        var obj = Instantiate(TrackSectionPrefab);
        obj.transform.parent = MeasureContainer;

        /// Get and configure script
        AmpTrackSection measure = obj.GetComponent<AmpTrackSection>();
        measure.ID = meta.ID;
        measure.Instrument = Instrument;
        measure.Length = SongController.measureLengthInzPos;

        // TODO: possibly simplify position &/ rotation properties?
        Vector3 measurePos = new Vector3(
            RhythmicGame.TrackWidth * ID, // X is the track's horizontal position
            0, meta.StartDistance); // Z is the distance at which the measure needs to be placed

        measure.PositionOnPath = measurePos;
        measure.PositionOnPath.z = measurePos.z;

        /// Add measure to measure list
        Measures.Add(measure);
        return measure;
    }

    public AmpNote CreateNote(MetaNote meta)
    {
        /// Create object
        var obj = Instantiate(NotePrefab);

        //obj.transform.position = Path.GetPointAtDistance(meta.Distance) + 
        //    Vector3.right * GetLocalXPosFromLaneType(meta.Lane) + 
        //    Vector3.right * (ID * RhythmicGame.TrackWidth);
        obj.transform.position = Path.GetPointAtDistance(meta.Distance);
        obj.transform.rotation = Path.GetRotationAtDistance(meta.Distance) * Quaternion.Euler(0, 0, 90);
        // TODO: Why does this work but directly changing position like above doesn't?
        obj.transform.Translate(Vector3.right * (GetLocalXPosFromLaneType(meta.Lane) + (RhythmicGame.TrackWidth * ID)));

        // set up
        obj.name = meta.Name;
        AmpNote note = obj.GetComponent<AmpNote>();

        return note;
    }
}

public class MetaMeasure
{
    public int ID;
    public AmpTrack.InstrumentType Instrument;
    public bool IsCaptured;
    public bool IsBossMeasure; // shouldn't capture this measure when capturing a track from another measure
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