using UnityEngine;

// Factory class for generating measures and notes in an AmpTrack

public partial class AmpTrack
{
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
        /// Create object
        var obj = Instantiate(TrackSectionPrefab);
        obj.transform.parent = MeasureContainer;

        /// Configure component
        AmpTrackSection measure = obj.GetComponent<AmpTrackSection>();
        measure.Track = this;
        measure.ID = meta.ID;
        measure.Instrument = Instrument;
        measure.Length = SongController.measureLengthInzPos;
        measure.IsCaptured = meta.IsCaptured;

        // Assign materials
        // TODO: improve!
        Material[] modelMaterials = new Material[3];
        modelMaterials[0] = TrackMaterial;
        modelMaterials[1] = LocalEdgeLightsMaterial;
        modelMaterials[2] = GlobalEdgeLightsMaterial;
        measure.ModelRenderer.materials = modelMaterials;

        measure.GlobalEdgeLightMaterial = GlobalEdgeLightsMaterial;

        //measure.ActiveSurfaceMeshRenderer.material = TrackMaterial_Active;

        //measure.EdgeLights_Local.MeshRenderer.material = LocalEdgeLightsMaterial;

        measure.EdgeLightsColor = Color;

        // TODO: possibly simplify position &/ rotation properties?
        measure.Position = new Vector3(
            TunnelPos.x, // X is the track's horizontal position
            TunnelPos.y, meta.StartDistance); // Z is the distance at which the measure needs to be placed
        measure.Rotation = TunnelRot.z;

        /// Add measure to measure list
        Measures.Add(measure);
        return measure;
    }
    public AmpNote CreateNote(MetaNote meta, AmpTrackSection measure = null, int id = 0, bool lastNote = false)
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
        note.ID = id;
        note.TotalID = meta.TotalID;
        note.SharedNoteMaterial = TracksController.SharedNoteMaterial;
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

// TODO: structs!
public class MetaMeasure
{
    public int ID;
    public AmpTrack.InstrumentType Instrument;
    public bool IsCaptured;
    public bool IsBossMeasure; // shouldn't capture this measure when capturing a track from another measure
    public float StartDistance;
}

public class MetaNote
{
    public string Name;
    public int TotalID;
    public NoteType Type;
    public LaneSide Lane;
    public int MeasureID;
    public float Distance;
    public bool IsCaptured;
}