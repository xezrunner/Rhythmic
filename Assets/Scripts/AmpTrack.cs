using UnityEngine;

public class AmpTrack : MonoBehaviour
{
    /// References to the contents

    /// Path-related stuff
    public PathCreation.PathCreator PathCreator;
    public PathCreation.VertexPath Path
    {
        get
        {
            if (PathCreator) return PathCreator.path;
            else { Debug.LogError("AmpTrack: Path not available - PathCreator is null!"); return null; }
        }
    }

    /// Global variables and properties

    bool _isEnabled;
    public bool IsEnabled // Disable notes within track, grey out everything
    {
        get { return _isEnabled; }
        set { IsEnabled = true; /* additional logic here... */ }
    }

    // Compatibility backport from OG Track
    // TODO: change functionality in TUT?
    bool _TUT_IsTrackEnabled;
    public bool TUT_IsTrackEnabled { get; set; /* TBA */ }

    bool _isTrackFocused;
    public bool IsTrackFocused { get; set; /* TBA */ }

    bool _isTrackCaptured;
    public bool IsTrackCaptured { get; set; /* TBA */ }

    bool _isTrackCapturing;
    public bool IsTrackCapturing { get; set; /* TBA */ }

    public bool IsTrackEmpty { get; /* TBA */ }
    public bool HideEmptyMeasures = true; // Whether empty measures should leave a hole
}