using UnityEngine;

// Generating measures and notes in an AmpTrack

public partial class AmpTrack
{
    TrackStreamer TrackStreamer { get { return TrackStreamer.Instance; } }

    [Header("Prefabs")]
    public GameObject TrackSectionPrefab;
    public GameObject NotePrefab;

    public static AmpTrack CreateTrack(int ID, string name, InstrumentType instrument, int realID, bool isCloneTrack, int trackSetID)
    {
        var obj = Instantiate(TracksController.Instance.TrackPrefab, TracksController.Instance.gameObject.transform);
        obj.name = name + (isCloneTrack ? $"F{realID}" : "");

        // Add Track component:
        AmpTrack com = obj.GetComponent<AmpTrack>();

        com.ID = ID;
        com.RealID = realID;
        com.TrackName = name;
        com.IsCloneTrack = isCloneTrack;
        com.TrackSetID = trackSetID;
        com.Instrument = instrument;
        com.Path = PathTools.Path;

        return com;
    }
    public AmpTrackSection CreateMeasure(MetaMeasure meta)
    {
        AmpTrackSection measure = TrackStreamer.GetDestroyedMeasure(RealID);
        GameObject obj = measure ? measure.gameObject : null;
        if (!measure) // If we didn't get a recycle-able measure back, create a new one!
        {
            obj = Instantiate(TrackSectionPrefab);
            measure = obj.GetComponent<AmpTrackSection>();
        }
        obj.transform.parent = MeasureContainer;

        // Configure component
        measure.Track = this;
        measure.ID = meta.ID;
        measure.Instrument = Instrument;
        measure.Length = SongController.measureLengthInzPos;
        measure.IsEmpty = meta.IsEmpty;
        measure.IsCaptured = meta.IsCaptured;

        // Assign materials | TODO: improve!
        //Material[] modelMaterials = new Material[2]
        //    { Track_Bottom_Global_Mat, Track_Bottom_Mat };
        //measure.ModelRenderer.materials = modelMaterials;

        // TODO: possibly simplify position &/ rotation properties?
        measure.Position = new Vector3(
            TunnelPos.x, // X is the track's horizontal position
            TunnelPos.y, meta.StartDistance); // Z is the distance at which the measure needs to be placed
        measure.Rotation = TunnelRot.z;
        measure.RotationQuat = Quaternion.Euler(TunnelRot); // Used in AmpTrackDestructFX

        measure.Start();

        /// Add measure to measure list
        Measures.Add(measure);
        return measure;
    }
    public AmpNote CreateNote(MetaNote meta, AmpTrackSection measure = null, int id = 0, bool lastNote = false)
    {
        AmpNote note = TrackStreamer.GetDestroyedNote();
        GameObject obj = note ? note.gameObject : null;
        if (!note)
        {
            obj = Instantiate(NotePrefab);
            if (measure) obj.transform.parent = measure.NotesContainer;
            note = obj.GetComponent<AmpNote>();
        }

        Vector3 offset = TunnelPos - Tunnel.center;
        obj.transform.position = PathTools.GetPositionOnPath(Path, meta.Distance, offset);
        obj.transform.rotation = PathTools.GetRotationOnPath(Path, meta.Distance, TunnelRot);

        Vector3 localRight = obj.transform.right; // After rotating the object on the path, we have the normal direction to the right
        Vector3 laneOffset = localRight * GetLocalXPosFromLaneType(meta.Lane);

        obj.transform.Translate(laneOffset, Space.World); // Translate to lane
        obj.transform.Translate(obj.transform.up * 0.05f, Space.Self);

        // set up
        obj.name = meta.Name;
        note.ID = id;
        note.TotalID = meta.TotalID;
        note.Track = this;
        note.SharedNoteMaterial = TracksController.SharedNoteMaterial;
        note.DotLightMeshRenderer.material = NoteDotLightMaterial;
        note.DotLightColor = Color;
        note.TrackID = RealID;
        note.MeasureID = meta.MeasureID;
        note.Lane = meta.Lane;
        note.Distance = meta.Distance;
        note.TimeMs = meta.TimeMs;
        note.IsCaptured = meta.IsCaptured;
        note.IsLastNote = lastNote;

        note.Start();

        measure.Notes.Add(note);
        return note;
    }
}

// TODO: structs!
public struct MetaMeasure
{
    public int ID;
    public bool IsEmpty;
    public bool IsCaptured;
    public bool IsBossMeasure; // shouldn't capture this measure when capturing a track from another measure
    public float StartDistance;
}

public struct MetaNote
{
    public string Name;
    public int TotalID;
    public int TrackID;
    public int MeasureID;
    public float Distance;
    public float TimeMs;
    public NoteType Type;
    public LaneSide Lane;

    public bool IsCaptured;
    public bool IsTargetNote;
}