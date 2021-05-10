using TMPro;
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

public class Note : MonoBehaviour
{
    /// References to the contents
    public GameObject Note_Object;
    public GameObject DotLight;
    public MeshRenderer NoteMeshRenderer;
    public MeshRenderer DotLightMeshRenderer;
    public ParticleSystem.MainModule PS_main;
    public ParticleSystem PS;

    public static bool DEBUG_ShowTargetNoteIndicators = false;
    public Canvas UI_Canvas;
    public TextMeshProUGUI TargetNoteIndicator;

    /// Materials (TODO: revise)
    public Material SharedNoteMaterial;

    /// Colors
    Color _color = Color.white;
    public Color Color
    {
        get { return _color; }
        set
        {
            if (RhythmicGame.IsTunnelTrackDuplication)
                foreach (Track t in Track.TrackTwins)
                {
                    var n = t.Measures[MeasureID].Notes[ID];
                    if (!n) return;
                    n.NoteMeshRenderer.material.color = value;
                    n.NoteMeshRenderer.material.SetFloat("_PlaneEnabled", 0);
                }

            NoteMeshRenderer.material.color = value;
            NoteMeshRenderer.material.SetFloat("_PlaneEnabled", 0);
        }
    }

    Color _dotLightColor = Color.white;
    public Color DotLightColor
    {
        // TODO: We'll need an optimized way to handle dot light colors for either:
        // a, individual notes, or, more likely: b, individual *tracks*
        get { return _dotLightColor; }
        set { _dotLightColor = value; /*DotLightMeshRenderer.sharedMaterial.color = value;*/ }
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
    public int ID;
    public int TotalID;
    public Track Track;
    public int TrackID;
    public int MeasureID;
    public NoteType NoteType;
    public LaneSide Lane = LaneSide.Center;
    public float Distance;
    public float TimeMs;
    public bool IsLastNote;
    public PowerupType PowerupType;

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
            // TODO: destruct_hit!!!
        }
    }

    public void ResetComponent()
    {
        IsLastNote = false;
        IsEnabled = true;
        IsCaptured = false;
    }
    public void Start()
    {
        if (SharedNoteMaterial)
            NoteMeshRenderer.material = SharedNoteMaterial;

        // Setup dotlight
        // TODO: cleanup!
        //DotLight.SetActive(false);
        DotLight.transform.localPosition = Vector3.zero;
        //DotLightColor = AmpTrack.Colors.ColorFromInstrument(TracksController.Instance.Tracks[TrackID].Instrument);
        //DotLightGlowIntensity = 1f;

        // Set particle system color to match dotlight!
        PS_main = PS.main;
        PS_main.startColor = (DotLightColor * 2f);

        if (PowerupType > 0)
        {
            NoteMeshRenderer.material.color = Colors.ConvertToFloatColor(new Color(128,0,128));
            UI_Canvas.gameObject.SetActive(true);
            TargetNoteIndicator.text = $"Powerup: {PowerupType}";
        }

        UI_Canvas.gameObject.SetActive(DEBUG_ShowTargetNoteIndicators);
    }

#if false
    private void Update()
    {
        if (!DEBUG_ShowTargetNoteIndicators) { UI_Canvas.gameObject.SetActive(false); return; }

        if (TracksController.Instance.targetNotes[Track.ID] == this)
        { UI_Canvas.gameObject.SetActive(true); TargetNoteIndicator.gameObject.SetActive(true); TargetNoteIndicator.text = $"{TotalID}@{Distance}m"; }
        else
            TargetNoteIndicator.gameObject.SetActive(false);
    }
#endif

    [SerializeField] NoteFX FXCom;

    /// <param name="twins">Whether to capture twin tracks' notes too.</param>
    public void CaptureNote(NoteCaptureFX fx = NoteCaptureFX.DestructCapture, bool twins = false, bool anim = true)
    {
        IsCaptured = true;

        if (!anim) return;

        if (!FXCom) // Add capture effect provider component
        {
            FXCom = gameObject.AddComponent<NoteFX>();
            FXCom.Note = this;
        }
        else FXCom.ResetFX(); // If it already exists, reset the effect
        FXCom.Play(fx); // Set capture effect

        if (twins && RhythmicGame.IsTunnelTrackDuplication) // Capture twin tracks' notes too
            foreach (Track t in Track.TrackTwins) t.Measures[MeasureID].Notes[ID].CaptureNote(fx, anim: anim);
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