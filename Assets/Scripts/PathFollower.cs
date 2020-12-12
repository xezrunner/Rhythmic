using PathCreation;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// Original: https://assetstore.unity.com/packages/tools/utilities/b-zier-path-creator-136082
// Modified for Rhythmic by xezrunner @ XesignSoftware vInc.

// Moves along a path at a constant speed.
// Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.

// TODO: ***** Change implementation to suit Rhythmic song movement! ******

public class PathFollower : MonoBehaviour
{
    Player Player { get { return Player.Instance; } }
    SongController SongController { get { return SongController.Instance; } }

    public PathCreator pathCreator; // The PathCreator to follow
    public EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop; // The instruction to perform when we reached the end of the path.

    public float speed = 5f;
    public float smoothStrength = 0.1f;
    public float distanceTravelled;
    public Quaternion Qvel = Quaternion.identity;
    public Vector3 Vvel = Vector3.zero;

    public Transform ContentContainer;

    public Transform NonInterpolatable;
    public Transform Interpolatable;

    public GameObject NotePrefab;
    public GameObject Plane;

    IEnumerator LoadStepValue()
    {
        while (float.IsNaN(SongController.secInzPos))
            yield return null;
    }

    void Awake()
    {
        if (pathCreator == null)
        { Debug.LogError("PathFollower: No PathCreator to follow!"); return; }

        // Subscribe to the pathUpdated event so that we're notified if the path changes during the game
        pathCreator.pathUpdated += OnPathChanged;
    }

    void Start()
    {
        StartCoroutine(LoadStepValue());

        TransformPlayerToPath(0f); // Position player to start of the path

        //await Task.Delay(1);
        //TransformPlayerToPath(AmpTrackController.Instance.Tracks[0].Measures[0].PositionOnPath.z); // Position player to start of the path
        //distanceTravelled = AmpTrackController.Instance.Tracks[0].Measures[0].PositionOnPath.z;

        // UNUSED CODE - TEMP TRACK TEST RANDOM NOTE PLACEMENT
        //int trackCounter = 0;
        //foreach (GameObject trGo in TrackMeshCreator.Instance.TrackObjects)
        //{
        //    for (int i = 0; i < Random.Range(150, 250); i++)
        //    {
        //        var go = Instantiate(NotePrefab);
        //        float distance = Random.Range(0, pathCreator.path.length);
        //        go.transform.rotation = pathCreator.path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90);
        //        go.transform.position = pathCreator.path.GetPointAtDistance(distance);
        //        go.transform.GetChild(0).gameObject.SetActive(false);
        //        go.transform.Translate(Vector3.right * (Track.GetLocalXPosFromLaneType((Track.LaneType)Random.Range(0, 3)) + (RhythmicGame.TrackWidth * trackCounter)));
        //        go.transform.parent = trGo.transform;
        //    }
        //    trackCounter++;
        //}
    }

    public float offset;
    public void TransformPlayerToPath(float dist)
    {
        //if (dist < pathCreator.path.localPoints[0].z)
        //{
        //    transform.position = new Vector3(0,0,dist);
        //    return;
        //}

        Vector3 localRight = pathCreator.path.GetNormalAtDistance(dist, endOfPathInstruction);
        //Vector3 currentPos = transform.position;
        Vector3 targetPos = pathCreator.path.GetPointAtDistance(dist, endOfPathInstruction) + localRight * Mathf.Abs(offset);

        transform.position = targetPos;

        Quaternion currentRot = Interpolatable.rotation;
        Quaternion targetRot = pathCreator.path.GetRotationAtDistance(dist, endOfPathInstruction) * Quaternion.Euler(0, 0, 90);
        Interpolatable.rotation = QuaternionUtil.SmoothDamp(currentRot, targetRot, ref Qvel, smoothStrength);
        NonInterpolatable.rotation = targetRot;
    }

    public float step;

    float legitOffset = 0f;
    float offsetSmoothCurrent = 0f;
    void Update()
    {
        if (pathCreator == null) return;

        TransformPlayerToPath(distanceTravelled + SongController.SecTozPos(RhythmicGame.AVCalibrationOffsetMs / 1000));
        TransformPlayerToPath(distanceTravelled + SongController.msInzPos * RhythmicGame.AVCalibrationOffsetMs);

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            legitOffset += RhythmicGame.TrackWidth;

            if (legitOffset > 6 * RhythmicGame.TrackWidth) legitOffset = 0;

            foreach (AmpTrack track in AmpTrackController.Instance.Tracks)
            {
                if (track.ID == Mathf.RoundToInt(legitOffset / RhythmicGame.TrackWidth))
                {
                    foreach (AmpTrackSection s in track.Measures)
                        if (s && s.IsEmpty)
                            s.EdgeLights_Local.gameObject.SetActive(true);
                }
                else
                    foreach (AmpTrackSection s in track.Measures)
                        if (s && s.IsEmpty)
                            s.EdgeLights_Local.gameObject.SetActive(false);
            }
        }

        offset = Mathf.SmoothDamp(offset, legitOffset, ref offsetSmoothCurrent, 1f, 100f, 10f * Time.deltaTime);

        if (!Player.IsPlaying) return;

        step = (Player.PlayerSpeed * SongController.secInzPos) * Time.unscaledDeltaTime * SongController.songSpeed;

        if (SongController.Enabled)
            distanceTravelled = Mathf.MoveTowards(distanceTravelled, float.MaxValue, step);
        else
            distanceTravelled += speed * Time.deltaTime;

        //Debug.Log("Target: " + targetRot);
    }

    void Rotate(Quaternion startRot, Quaternion endRot, float rotateTime)
    {
        var i = 0.0f;
        var rate = 1.0f / rotateTime;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            transform.rotation = Quaternion.Lerp(startRot, endRot, Mathf.SmoothStep(0.0f, 1.0f, i));
        }
    }

    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged() => distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
}