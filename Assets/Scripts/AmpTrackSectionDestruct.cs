using PathCreation;
using System.Collections.Generic;
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
    ClipManager ClipManager;

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

        ClipManager = track.ClipManager;
        ClipManager.inverse_plane = ClipPlane; // Assign inverse clip plane

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
    void Start() { if (!measure.IsCaptured & !measure.IsCapturing) Clip(); } // Clip to start (0f)

    [Range(0, 1)]
    public float fraction;

    List<int> lastCapturedNotes = new List<int>();

    private void Update()
    {
        if (fraction != 1f)
        {
            fraction = Mathf.MoveTowards(fraction, 1.0f, track.captureAnimStep * Time.deltaTime);

            // Capture notes
            for (int i = 0; i < fraction * measure.Notes.Count; i++)
                if (!lastCapturedNotes.Contains(i))
                {
                    measure.Notes[i].CaptureNote();
                    lastCapturedNotes.Add(i);
                }

        }
        else // 1f, done
        {
            measure.IsCapturing = false;
            measure.IsCaptured = true;
            Destroy(this);
        }

        // Don't update clipping if the measure was already captured!
        // Do continue the rest of the capturing though, as we don't want to skip measures during capturing.
        if (measure.CaptureState != MeasureCaptureState.Captured)
            Clip(fraction);
    }

    public void Clip(float fraction = 0f)
    {
        Mathf.Clamp01(fraction); // Clamp between 0 and 1

        // Calculate clip plane offset based on fraction
        float dist = PositionOnPath.z + (Length * fraction);

        //if (fraction == 1f)
        //    dist += 1f; // wtf?

        Vector3 planePos = PathTools.GetPositionOnPath(Path, dist);
        Quaternion planeRot = PathTools.GetRotationOnPath(Path, dist, new Vector3(90, 0, 0));

        ClipPlane.transform.position = planePos;
        ClipPlane.transform.rotation = planeRot;

        ClipManager.Clip(); // update clipping
    }
}
