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

    AmpTrackSection Measure;
    AmpTrack Track;
    ClipManager ClipManager;
    AmpTrackDestructFX DestructFX;

    public void Init(AmpTrackSection m)
    {
        Measure = m;
        Track = Measure.Track;

        ID = m.ID;
        Path = m.Path;
        ClipPlane = m.ClipPlane; // Get capture clipping plane from AmpTrackSection
        PositionOnPath = m.Position;
        RotationOnPath = m.Rotation;
        Length = m.Length;

        ClipManager = Track.ClipManager;
        ClipManager.inverse_plane = ClipPlane; // Assign inverse clip plane
        DestructFX = m.DestructFX;

        Measure.IsCapturing = true;

        // Set fraction to current measure progress
        if (Mathf.FloorToInt(Clock.Instance.bar) == ID)
            fraction = Mathf.Clamp(Clock.Instance.bar - ID /*- 0.08f*/, 0f, 1f);

        // Add destruct FX if non-existent:
        // NOTE: it should be parented to the measure, as we get destroyed once the FX finishes!
        if (!DestructFX)
        {
            // Load and add prefab, etc...
        }

        // Set up & start destruct FX:
        if (DestructFX && !m.IsEmpty)
        {
            DestructFX.TrackColor = m.Track.Color;
            DestructFX.gameObject.SetActive(true);
            DestructFX.Play();
        }
    }

    void Awake()
    {
        // If not initialized with a measure, get it!
        if (!Measure)
            Init(GetComponent<AmpTrackSection>());
    }
    void Start() { if (!Measure.IsCaptured & !Measure.IsCapturing) Clip(); } // Clip to start (0f)

    [Range(0, 1)]
    public float fraction;

    List<int> lastCapturedNotes = new List<int>();

    Vector3 pathPos;
    Quaternion pathRot;

    private void Update()
    {
        float dist = 0f;

        if (Measure.CaptureState != MeasureCaptureState.Captured)
        {
            // Calculate path distance based on fraction
            dist = PositionOnPath.z + (Length * fraction);

            // Update pos along path
            pathPos = PathTools.GetPositionOnPath(Path, dist);
            pathRot = PathTools.GetRotationOnPath(Path, dist);

            // Don't update clipping if the measure was already captured!
            // Do continue the rest of the capturing though, as we don't want to skip measures during capturing.
            if (Measure.CaptureState != MeasureCaptureState.Captured)
                Clip();
        }

        if (fraction != 1f)
        {
            fraction = Mathf.MoveTowards(fraction, 1.0f, Track.captureAnimStep * Time.deltaTime);

            // Capture notes
            for (int i = 0; i < fraction * Measure.Notes.Count; i++)
            {
                if (!lastCapturedNotes.Contains(i))
                {
                    Measure.Notes[i].CaptureNote();
                    lastCapturedNotes.Add(i);
                }
            }

            // Update position for capture FX!
            {
                // TODO: PathTools.GetPositionOnPath() could provide out values so we don't need to get it here
                Vector3 normalRight = (Path != null) ? Path.GetNormalAtDistance(dist) : Vector3.right;
                Vector3 normalUp = Vector3.Cross(Path.GetTangentAtDistance(dist), normalRight);

                Vector3 pos = pathPos + (normalRight * PositionOnPath.x)
                                      + (normalUp * PositionOnPath.y);

                DestructFX.transform.position = pos;
                DestructFX.transform.rotation = pathRot;
            }
        }
        else // 1f, done!
        {
            DestructFX.Stop();
            Measure.IsCapturing = false;
            Measure.IsCaptured = true;
            Destroy(this);
        }
    }

    public void Clip()
    {
        //if (fraction == 1f)
        //    dist += 1f; // TODO: wtf?

        ClipPlane.transform.position = pathPos;
        ClipPlane.transform.rotation = pathRot * Quaternion.Euler(new Vector3(90, 0, 0));

        ClipManager.Clip(); // update clipping
    }
}
