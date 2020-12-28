using PathCreation;
using UnityEngine;

/// AmpTrackSectionDestruct
// This is a single measure of a track ** BEING DESTROYED **. Its regular script is removed.

public class AmpTrackSectionDestruct : MonoBehaviour
{
    VertexPath Path;
    GameObject ClipPlane;

    int ID;
    float Length = 32f;
    Vector3 PositionOnPath;
    float RotationOnPath; // Note: in Euler angles!

    AmpTrackSection measure;
    AmpTrack track;
    ClippingPlane ClipManager;

    public bool IsCapturing;

    public void Init(AmpTrackSection _measure)
    {
        this.measure = _measure;
        track = measure.Track;

        ID = _measure.ID;
        Path = _measure.Path;
        ClipPlane = _measure.ClipPlane; // Get capture clipping plane from AmpTrackSection
        PositionOnPath = _measure.Position;
        RotationOnPath = _measure.Rotation;
        Length = _measure.Length;

        ClipManager = _measure.ClipManager;
        ClipManager.inverse_plane = ClipPlane; // Assign inverse clip plane

        // TODO: only measure.IsCaptured is enough?
        measure.IsCapturing = true;

        // Set fraction to current measure progress
        if (Mathf.FloorToInt(Clock.Instance.bar) == ID)
            fraction = Mathf.Clamp(Clock.Instance.bar - ID - 0.08f, 0f, 1f);
    }

    void Awake()
    {
        // If not initialized with a measure, get it!
        if (!measure)
            Init(GetComponent<AmpTrackSection>());
    }
    void Start() { if (!measure.IsCaptured) Clip(); } // Clip to start (0f)

    [Range(0, 1)]
    public float fraction;

    private void Update()
    {
        if (fraction != 1f)
        {
            fraction = Mathf.MoveTowards(fraction, 1.0f, track.captureAnimStep * Time.deltaTime);

            // Capture notes
            for (int i = 0; i < fraction * measure.Notes.Count; i++)
                    measure.Notes[i].CaptureNote();
        }
        else
        {
            measure.IsCapturing = false;
            measure.IsCaptured = true;
            Destroy(this);
        }

        // Don't update clipping if the measure was already captured!
        // Do continue the rest of the capturing though, as we don't want to skip measures during capturing.
        if (!measure.IsCaptured)
            Clip(fraction);
    }

    public void Clip(float fraction = 0f)
    {
        Mathf.Clamp01(fraction); // Clamp between 0 and 1

        // Calculate clip plane offset based on fraction
        float offset = Length * fraction;

        if (fraction == 1f)
            offset += 1f; // wtf?

        Vector3 localUp = Vector3.Cross(Path.GetTangentAtDistance(PositionOnPath.z + offset), Path.GetNormalAtDistance(PositionOnPath.z + offset));
        Vector3 localRight = Path.GetNormalAtDistance(PositionOnPath.z + offset);

        Vector3 planePos = Path.GetPointAtDistance(PositionOnPath.z + offset);
        planePos += (localRight * PositionOnPath.x) + (localUp * PositionOnPath.y); // offset

        Quaternion planeRot = Path.GetRotationAtDistance(PositionOnPath.z + offset) * Quaternion.Euler(90, 0, 0);

        ClipPlane.transform.position = planePos;
        ClipPlane.transform.rotation = planeRot;

        ClipManager.Clip(); // update clipping
    }
}
