using System;
using System.Collections;
using UnityEngine;

public class Note : MonoBehaviour
{
    public enum NoteCatchType { Success = 0, Miss = 1 }
    public MeshRenderer DotLightMeshRenderer;
    public GameObject DotLight;
    Color _dotLightColor;
    public Color DotLightColor
    {
        get { return _dotLightColor; }
        set { _dotLightColor = value; }
    }

    //public string noteName { get { return gameObject.name; } set { gameObject.name = value; } }
    public NoteType noteType;
    public Track noteTrack;
    public Track.LaneType noteLane;
    public Measure noteMeasure;
    public int measureNum;
    public float zPos;

    public bool IsNoteActive = true; // Whether the note is greyed out (used when failing)
    public bool IsNoteCaptured = false;

    void Start()
    {
        // TODO: optimize!!!
        // Set up the dot light
        DotLight = transform.GetChild(0).gameObject;
        DotLight.SetActive(false); DotLight.transform.localPosition = Vector3.zero;
        DotLightMeshRenderer = DotLight.GetComponent<MeshRenderer>();

        Material mat = Instantiate(DotLightMeshRenderer.material);
        Color finalColor = Track.Colors.ConvertColor(DotLightColor);

        mat.color = finalColor;

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", finalColor * 1f);

        DotLightMeshRenderer.material = mat;
    }

    /// <summary>
    /// This event is invoked when the note is blasted.
    /// </summary>
    //public event EventHandler<NoteType> OnCatch;

    public void CaptureNote(NoteCatchType catchType = NoteCatchType.Success)
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        DotLight.SetActive(true);

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