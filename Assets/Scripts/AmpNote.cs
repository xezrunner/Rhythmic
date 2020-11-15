using UnityEngine;

/// Note entity
/// 
/// Planned features:
///   Tech:
///     - Restoration of note capturing (also Level Editor / Gameplay (practice mode)!)
///   Gameplay:
///     - Streak indicators: shows some hint to the player as to which note is the next one to hit. Could also show the next multiplier / [total score of sequence] amount.
///   Level Editor:
///     - Quantization of notes: automatically place notes on the beat. Should support different snapping settings as well!

public class AmpNote : MonoBehaviour
{
    /// References to the contents
    public GameObject Note;
    public GameObject DotLight;
    public MeshRenderer NoteMeshRenderer;
    public MeshRenderer DotLightMeshRenderer;

    /// Colors
    Color _dotLightColor = Color.white;
    public Color DotLightColor
    {
        get { return _dotLightColor; }
        set { _dotLightColor = value; DotLightMeshRenderer.material.color = value; }
    }

    /// Global variables and properties
    public int TrackID;
    public int MeasureID;
    public NoteType NoteType;
    public AmpTrack.LaneSide Lane = AmpTrack.LaneSide.Center;
    public float Distance;

    private bool _isEnabled;
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set { _isEnabled = value; /* TBA: gray out or restore note color!*/ }
    }

    private bool _isCaptured;
    public bool IsCaptured
    {
        get { return _isCaptured; }
        set { _isCaptured = value; }
    }

    void Start()
    {
        DotLight.SetActive(false); DotLight.transform.localPosition = Vector3.zero;

        // Setup dotlight
        // TODO: cleanup!
        DotLightColor = Colors.ConvertColor(AmpTrack.Colors.ColorFromTrackType(AmpTrackController.Instance.Tracks[TrackID].Instrument));
        DotLightMeshRenderer.material.SetColor("_EmissionColor", Colors.ConvertColor(DotLightColor) * 0.6f);

        // Set particle system color to match dotlight!
        var ps_main = transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>().main;
        ps_main.startColor = Colors.ConvertColor(DotLightColor * 1.5f);
    }

    public void CaptureNote(bool anim = true)
    {
        IsCaptured = true;

        if (!anim) // Ignore animation if specified
            DotLight.transform.GetChild(0).gameObject.SetActive(false);
    }
}

public enum NoteType
{
    Generic = 0, // a generic note
    Autoblaster = 1, // Cleanse
    Slowdown = 2, // Sedate
    Multiply = 3, // Multiply
    Freestyle = 4, // Flow
    Autopilot = 5, // Temporarily let the game play itself
    STORY_Corrupt = 6, // Avoid corrupted nanotech!
    STORY_Memory = 7 // Temporarily shows memories as per the lore
}