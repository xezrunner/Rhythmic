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
    public ParticleSystem.MainModule PS_main;
    public ParticleSystem PS;

    /// Colors
    Color _dotLightColor = Color.white;
    public Color DotLightColor
    {
        get { return _dotLightColor; }
        set { _dotLightColor = value; DotLightMeshRenderer.material.color = value; }
    }

    float _dotLightGlowIntensity = 1f;
    public float DotLightGlowIntensity
    {
        get { return _dotLightGlowIntensity; } 
        set
        {
            _dotLightGlowIntensity = value;
            DotLightMeshRenderer.material.SetColor("_EmissionColor", DotLightColor * value);
        }
    }

    /// Global variables and properties
    public AmpTrack Track;
    public int TrackID;
    public int MeasureID;
    public NoteType NoteType;
    public LaneSide Lane = LaneSide.Center;
    public float Distance;
    public bool IsLastNote;

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set
        {
            _isEnabled = value;
            NoteMeshRenderer.material.color = Color.gray;
        }
    }

    private bool _isCaptured;
    public bool IsCaptured
    {
        get { return _isCaptured; }
        set
        {
            _isCaptured = value;

            DotLight.SetActive(value);
            NoteMeshRenderer.enabled = !value;
        }
    }

    void Start()
    {
        // Setup dotlight
        // TODO: cleanup!
        //DotLight.SetActive(false);
        DotLight.transform.localPosition = Vector3.zero;
        DotLightColor = AmpTrack.Colors.ColorFromInstrument(TracksController.Instance.Tracks[TrackID].Instrument);
        DotLightGlowIntensity = 1f;

        // Set particle system color to match dotlight!
        PS_main = PS.main;
        PS_main.startColor = (DotLightColor * 2f);
    }

    [SerializeField] AmpNoteFX FXCom;

    public void CaptureNote(NoteCaptureFX fx = NoteCaptureFX.DestructCapture, bool anim = true)
    {
        IsCaptured = true;

        if (!anim) return;

        if (!FXCom) // Add capture effect provider component
            FXCom = gameObject.AddComponent<AmpNoteFX>();
        else FXCom.ResetFX(); // If it already exists, reset the effect
        FXCom.Note = this;
        FXCom.Effect = fx; // Set capture effect
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
    STORY_Memory = 7 // Temporarily shows memories
}