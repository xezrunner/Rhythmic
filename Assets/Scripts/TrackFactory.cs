using UnityEngine;

// Generating measures and notes in an AmpTrack

public partial class Track
{
    TrackStreamer TrackStreamer { get { return TrackStreamer.Instance; } }

    [Header("Prefabs")]
    public GameObject TrackSectionPrefab;
    public GameObject NotePrefab;

    public static Track CreateTrack(int ID, string name, InstrumentType instrument, int realID, bool isCloneTrack, int trackSetID)
    {
        var obj = Instantiate(TracksController.Instance.TrackPrefab, TracksController.Instance.gameObject.transform);
        obj.name = name + (isCloneTrack ? $"F{realID}" : "");

        // Add Track component:
        Track com = obj.GetComponent<Track>();

        com.ID = ID;
        com.RealID = realID;
        com.TrackName = name;
        com.IsCloneTrack = isCloneTrack;
        com.TrackSetID = trackSetID;
        com.Instrument = instrument;
        com.Path = PathTools.Path;

        return com;
    }
    public Measure CreateMeasure(MetaMeasure meta)
    {
        Measure measure = TrackStreamer.GetDestroyedMeasure(RealID);
        GameObject obj = measure ? measure.gameObject : null;
        if (!measure) // If we didn't get a recycle-able measure back, create a new one!
        {
            obj = Instantiate(TrackSectionPrefab);
            measure = obj.GetComponent<Measure>();
        }
        obj.transform.parent = MeasureContainer;

        // Configure component
        measure.Track = this;
        measure.ID = meta.ID;
        measure.Instrument = Instrument;
        measure.Length = SongController.bar_length_pos; // TODO: We should have a variable like 'bar_length_pos' in SongTimeUnits
        measure.IsEmpty = meta.IsEmpty;
        measure.IsCaptured = meta.IsCaptured;

        // assign materials | todo: improve!
        //material[] modelmaterials = new material[2]
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
    public Note CreateNote(MetaNote meta, Measure measure = null, int id = 0, bool last_note = false, PowerupType powerup_type = PowerupType.None)
    {
        Note note = TrackStreamer.GetDestroyedNote();
        GameObject obj = note ? note.gameObject : null;
        if (!note)
        {
            obj = Instantiate(NotePrefab);
            if (measure) obj.transform.parent = measure.NotesContainer;
            note = obj.GetComponent<Note>();
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
        note.IsLastNote = last_note;
        note.PowerupType = powerup_type;

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
    public PowerupType Powerup;
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