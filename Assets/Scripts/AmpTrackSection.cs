using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// AmpTrackSection (previously Measure)
/// This is a single measure of a track.

public class AmpTrackSection : MonoBehaviour
{
    /// References to the contents
    public GameObject LengthPlane; // This plane trims the model to the desired length.

    /// Path-related stuff
    public PathCreation.PathCreator PathCreator; // TODO: Global property? !!!
    public PathCreation.VertexPath Path { get { if (PathCreator) return PathCreator.path; else { Debug.LogError("AmpTrack: Path not available - PathCreator is null!"); return null; } } }

    // TODO: make these properties that'll move the track along the path!
    public Vector3 PositionOnPath;
    public Vector3 RotationOnPath; // Note: in Euler angles!

    /// Global variables and properties
    public float Length; // in zPos!

    /// Model functions
    void DeformMeshToPath(float length, Vector3 position) // Deforms the mesh at the given position and length
    {

    }    
}
