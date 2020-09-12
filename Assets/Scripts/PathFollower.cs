using PathCreation;
using UnityEngine;

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
    public float distanceTravelled;

    void Awake()
    {
        if (pathCreator == null)
        { Debug.LogError("PathFollower: No PathCreator to follow!"); return; }

        // Subscribe to the pathUpdated event so that we're notified if the path changes during the game
        pathCreator.pathUpdated += OnPathChanged;
    }

    void Update()
    {
        if (pathCreator == null) return;

        // TODO: This might end up being controlled by the Player
        distanceTravelled += speed * Time.deltaTime;

        // Position player along the path (pos & rot)
        transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
        // The path normals face 90 degrees to the left to make the path.
        // Here, we get the rotation but rotate the result by 90 degrees to the right to correctly orient the follower on the path.
        transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.Euler(0, 0, 90);

        
    }

    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged() => distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
}