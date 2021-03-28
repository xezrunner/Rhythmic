using PathCreation;
using System.Collections.Generic;
using UnityEngine;

/// AmpTrackSectionDestruct
// This is a single measure of a track ** BEING DESTROYED **. Its regular script is removed.

public class AmpTrackSectionDestruct : MonoBehaviour
{
    Clock Clock { get { return Clock.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
    FXProperties FXProps { get { return FXProperties.Instance; } }

    VertexPath Path;
    GameObject ClipPlane;

    int ID;
    float Length = 32f;
    Vector3 PositionOnPath;
    float RotationOnPath; // Note: in Euler angles!
    public Quaternion RotationQuat; // angles

    AmpTrackSection Measure;
    AmpTrack Track;
    ClipManager ClipManager;
    AmpTrackDestructFX DestructFX;

    // TODO TODO TODO:
    // Move this to be on a Track rather than a Measure!

    void Awake()
    {
        // If not initialized with a measure, get it!
        if (!Measure)
            Init(GetComponent<AmpTrackSection>());
    }
    void Start() { if (!Measure.IsCaptured & !Measure.IsCapturing) Clip(); } // Clip to start (0f)

    public void Init(AmpTrackSection m)
    {
        Measure = m;
        Track = Measure.Track;

        ID = m.ID;
        Path = m.Path;
        ClipPlane = m.ClipPlane; // Get capture clipping plane from AmpTrackSection
        PositionOnPath = m.Position;
        RotationOnPath = m.Rotation;
        RotationQuat = m.RotationQuat;
        Length = m.Length;

        ClipManager = Track.ClipManager;
        ClipManager.inverse_plane = ClipPlane; // Assign inverse clip plane

        Measure.IsCapturing = true;

        // Set fraction to current measure progress
        if (Mathf.FloorToInt(Clock.Instance.bar) == ID)
            fraction = Mathf.Clamp(Clock.Instance.bar - ID /*- 0.08f*/, 0f, 1f);

        //DestructFX = m.DestructFX;
        DestructFX = m.Track.DestructFX;
        // Add destruct FX if non-existent:
        // NOTE: it should be parented to the measure, as we get destroyed once the FX finishes!
        if (!DestructFX)
        {
            // Load and add prefab, etc...
        }

        // Set up & start destruct FX:
        if (DestructFX && (FXProps.Destruct_ForceEffects || !m.IsEmpty))
        {
            //DestructFX.gameObject.SetActive(true);

            // If the distance between the current measure and the target measure is < than 3,
            // we consider the effect 'proximity'.
            // Some particles will not play when not in proximity to provide better framerate.
            bool isProximity = (m.ID - Clock.Fbar < 3);
            // Consider clone tracks as NOT proximity - do not play sparkles
            bool isCloneTrack = (m.Track.RealID > TracksController.MainTracks.Length);
            DestructFX.Play(!isCloneTrack && isProximity);
        }
    }

    [Range(0, 1)]
    public float fraction;

    List<int> lastCapturedNotes = new List<int>();

    Vector3 pathPos;
    Quaternion pathRot;

    float dist = 0f;
    private void Update()
    {
        if (fraction != 1f)
        {
            if (Measure.CaptureState != MeasureCaptureState.Captured)
            {
                // Calculate path distance based on fraction
                dist = PositionOnPath.z + (Length * fraction);

                // Update pos along path
                pathPos = PathTools.GetPositionOnPath(Path, dist);
                pathRot = PathTools.GetRotationOnPath(Path, dist);

                // Don't update clipping if the measure was already captured!
                // Do continue the rest of the capturing though, as we don't want to skip measures during capturing.
                //if (Measure.CaptureState != MeasureCaptureState.Captured)
                Clip();

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
                    Vector3 normalRight = (Path != null) ? Path.GetNormalAtDistance(Mathf.Clamp(dist, 0, Path.length - 0.01f)) : Vector3.right;
                    Vector3 normalUp = (Path != null) ? Vector3.Cross(Path.GetTangentAtDistance(Mathf.Clamp(dist, 0, Path.length - 0.01f)), normalRight) : Vector3.up;

                    Vector3 pos = pathPos + (normalRight * PositionOnPath.x)
                                          + (normalUp * PositionOnPath.y);

                    DestructFX.transform.position = pos;
                    DestructFX.transform.rotation = pathRot * RotationQuat;
                    //Debug.DrawLine(pos, ((DestructFX.transform.rotation * Quaternion.Euler(0, 0, -90)) * pos) * 5f, Color.white, 2); // visualize up normal
                }
            }

            fraction = Mathf.MoveTowards(fraction, 1.0f, Track.captureAnimStep * Time.deltaTime);
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
        if (fraction == 1f)
            dist += 1f; // TODO: wtf?

        ClipPlane.transform.position = pathPos;
        ClipPlane.transform.rotation = pathRot * Quaternion.Euler(new Vector3(90, 0, 0));

        ClipManager.Clip(); // update clipping
    }
}
