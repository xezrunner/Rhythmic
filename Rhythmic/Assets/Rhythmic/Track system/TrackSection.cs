using UnityEngine;
using static Logger;

public class TrackSection : MonoBehaviour
{
    public PathTransform path_transform;

    void Awake()
    {
        if (!path_transform && LogW("A TrackSection needs to have a PathTransform component to exist. Deleting.".T(this)))
            Destroy(gameObject);
    }

    public const string PREFAB_PATH = "Prefabs/Track/TrackSection";
    public static TrackSection CreateTrackSection()
    {
        return null;
    }
}