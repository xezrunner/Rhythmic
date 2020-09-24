using PathCreation;
using UnityEngine;
using UnityEngine.InputSystem;

// Original: https://assetstore.unity.com/packages/tools/utilities/b-zier-path-creator-136082
// Modified for Rhythmic by xezrunner @ XesignSoftware vInc.

// Moves along a path at a constant speed.
// Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.

// TODO: ***** Change implementation to suit Rhythmic song movement! ******

public class PathFollower : MonoBehaviour
{
    public PathCreator pathCreator; // The PathCreator to follow
    public EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop; // The instruction to perform when we reached the end of the path.

    public float speed = 5f;
    public float smoothStrength = 0.1f;
    public float distanceTravelled;
    public Quaternion vel = Quaternion.identity;

    public Transform ContentContainer;

    public Transform NonInterpolatable;
    public Transform Interpolatable;

    public GameObject NotePrefab;
    public GameObject Plane;

    void Awake()
    {
        if (pathCreator == null)
        { Debug.LogError("PathFollower: No PathCreator to follow!"); return; }

        // Subscribe to the pathUpdated event so that we're notified if the path changes during the game
        pathCreator.pathUpdated += OnPathChanged;
    }

    void Start()
    {
        return;
        int trackCounter = 0;
        foreach (GameObject trGo in TrackMeshCreator.Instance.TrackObjects)
        {
            for (int i = 0; i < Random.Range(150, 250); i++)
            {
                var go = Instantiate(NotePrefab);
                float distance = Random.Range(0, pathCreator.path.length);
                go.transform.rotation = pathCreator.path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90);
                go.transform.position = pathCreator.path.GetPointAtDistance(distance);
                go.transform.GetChild(0).gameObject.SetActive(false);
                go.transform.Translate(Vector3.right * (Track.GetLocalXPosFromLaneType((Track.LaneType)Random.Range(0, 3)) + (RhythmicGame.TrackWidth * trackCounter)));
                go.transform.parent = trGo.transform;
            }
            trackCounter++;
        }
    }

    public float offset;

    void Update()
    {
        if (pathCreator == null) return;

        // TODO: This might end up being controlled by the Player
        distanceTravelled += speed * Time.deltaTime;

        // Position player along the path (pos & rot)
        Vector3 localRight = pathCreator.path.GetNormalAtDistance(distanceTravelled, endOfPathInstruction);
        Vector3 finalPos = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + localRight * Mathf.Abs(offset);
        //transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + Vector3.right * offset;
        transform.position = finalPos;
        // The path normals face 90 degrees to the left to make the path.
        // Here, we get the rotation but rotate the result by 90 degrees to the right to correctly orient the follower on the path.
        Quaternion currentRot = Interpolatable.rotation;
        Quaternion targetRot = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.Euler(0, 0, 90);
        Interpolatable.rotation = QuaternionUtil.SmoothDamp(currentRot, targetRot, ref vel, smoothStrength);
        NonInterpolatable.rotation = targetRot;

        Plane.transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled + 64);
        Plane.transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled + 64) * Quaternion.Euler(90, 0, 0);

        if (Keyboard.current.gKey.wasPressedThisFrame)
            offset += RhythmicGame.TrackWidth;

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