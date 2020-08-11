using System;
using System.Collections;
using UnityEngine;

public class Note : MonoBehaviour
{
    public enum NoteCatchType { Success = 0, Miss = 1 }
    public BoxCollider NoteCollider;
    public MeshRenderer NoteMeshRenderer;
    public MeshRenderer DotLightMeshRenderer { get { return DotLight.GetComponent<MeshRenderer>(); } }
    public GameObject DotLight;

    Color _color;
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            NoteMeshRenderer.material.color = Colors.ConvertColor(value);
        }
    }

    Color _dotLightColor = Color.white;
    public Color DotLightColor
    {
        get { return _dotLightColor; }
        set { _dotLightColor = value; }
    }

    public string noteName { get { return gameObject.name; } set { gameObject.name = value; } }
    public NoteType noteType = NoteType.Generic;
    public Track noteTrack;
    public Track.LaneType noteLane;
    public Measure noteMeasure { get { return noteTrack.GetMeasureForZPos(zPos); } }
    public int measureNum;
    public int subbeatNum;
    public float zPos;

    // Wheather the note is the last one in its measure
    public bool IsLastNote { get { return noteMeasure.noteList.IndexOf(this) == noteMeasure.noteList.Count - 1; } }

    // Whether the note is greyed out (used during note missing)
    bool _isNoteEnabled = true;
    public bool IsNoteEnabled
    {
        get { return _isNoteEnabled; }
        set
        {
            _isNoteEnabled = value;

            if (value)
                Color = new Color(255, 255, 255);
            else
                Color = new Color(60, 60, 60);
        }
    }

    public bool IsNoteToBeCaptured = false;

    bool _isNoteCaptured = false;
    public bool IsNoteCaptured
    {
        get { return _isNoteCaptured; }
        set
        {
            _isNoteCaptured = value;
            IsNoteEnabled = !value;

            GetComponent<Collider>().enabled = !value;
            GetComponent<MeshRenderer>().enabled = !value;

            DotLight.SetActive(value);
        }
    }

    void Awake()
    {
        NoteMeshRenderer = GetComponent<MeshRenderer>();
        NoteCollider = GetComponent<BoxCollider>();

        /*
        DotLightMeshRenderer = DotLight.GetComponent<MeshRenderer>();

        Material mat = Instantiate(DotLightMeshRenderer.material);
        Color finalColor = Colors.ConvertColor(DotLightColor);

        mat.color = finalColor;

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", finalColor * 0.3f);

        DotLightMeshRenderer.material = mat;
        */
    }

    private void Start()
    {
        // TODO: optimize!!!
        // Set up the dot light
        DotLight = transform.GetChild(0).gameObject;
        DotLight.SetActive(false); DotLight.transform.localPosition = Vector3.zero;
        DotLightMeshRenderer.material.color = Colors.ConvertColor(DotLightColor);
        DotLightMeshRenderer.material.SetColor("_EmissionColor", Colors.ConvertColor(DotLightColor) * 0.6f);

        var ps_main = transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>().main;
        ps_main.startColor = Colors.ConvertColor(DotLightColor * 1.5f);
    }
    private void Update()
    {
        if (IsNoteEnabled == !IsNoteEnabled)
            Debug.DebugBreak();
    }

    /// <summary>
    /// This event is invoked when the note is blasted.
    /// </summary>
    //public event EventHandler<NoteType> OnCatch;

    public void CaptureNote(NoteCatchType catchType = NoteCatchType.Success)
    {
        IsNoteCaptured = true;
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
}