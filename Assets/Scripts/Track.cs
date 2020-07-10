using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NAudio.Midi;
using System;
using System.Linq.Expressions;

public class Track : MonoBehaviour
{
    public int? ID;
    public TrackType Instrument;
    public bool IsTrackActive = false; // TODO: Active and Inactive tracks (Regeneration)
    public bool IsTrackFocused { get; set; } = false;
    public TrackLane[] Lanes; // Lanes in this Track

    public string TrackName { get { return gameObject.name; } }

    void Start()
    {
        // Set track material
        SetTrackMaterialByInstrument(Instrument);

        // Create lane splits, if they haven't been created already
        if (Lanes.Length < 1)
            CreateLanes();
    }

    #region Materials
    public void SetTrackMaterial(string materialPath)
    {
        Material finalMat = (Material)Resources.Load(materialPath, typeof(Material));

        if (finalMat != null)
        {
            gameObject.GetComponent<Renderer>().material = finalMat;
            if (gameObject.GetComponent<TrackEdgeLightController>() != null)
            {
                gameObject.GetComponent<TrackEdgeLightController>().Color = Colors.ColorFromTrackType(Instrument);
            }
        }
        else
            Debug.LogErrorFormat("TRACK [{0}]: Cannot load material {1}", TrackName, materialPath);
    }

    public void SetTrackMaterialByInstrument(TrackType type)
    {
        gameObject.GetComponent<Renderer>().material = GetTrackMaterial(type);
        if (gameObject.GetComponent<TrackEdgeLightController>() != null)
        {
            gameObject.GetComponent<TrackEdgeLightController>().Color = Colors.ConvertColor(Colors.ColorFromTrackType(Instrument));
            gameObject.GetComponent<TrackEdgeLightController>().ApplyMaterial();
        }
    }

    public string TrackMaterialPath = "Materials/Tracks/";
    Material GetTrackMaterial(TrackType type)
    {
        Material finalMaterial;
        //string finalPath = TrackMaterialPath + type.ToString() + "Material";
        string finalPath = TrackMaterialPath + "TrackMaterial";

        try
        {
            finalMaterial = new Material(Shader.Find("Standard"));
            finalMaterial.color = Colors.ConvertColor(Colors.ColorFromTrackType(type));

            if (RhythmicGame.DebugTrackMaterialEvents)
                Debug.LogFormat(string.Format("TRACK [{0}]: Using material {1}", TrackName, finalPath));

            return finalMaterial;
        }
        catch
        {
            Debug.LogError(string.Format("TRACK [{0}]: Cannot find material {1}!", TrackName, finalPath));
            return null;
        }
    }
    #endregion

    /// <summary>
    /// This creates our Lanes in the Track.
    /// We want to create 3 lanes for Left, Center and Right.
    /// </summary>
    public void CreateLanes()
    {
        // TODO: track legth from song properties!
        Lanes = new TrackLane[3];

        for (int i = 0; i < Lanes.Length; i++)
        {
            // calculate position for the lane
            float xPosition = 0f;
            switch (i)
            {
                case 0:
                    xPosition = -(float)1 / (float)3; break;
                case 1:
                    xPosition = 0; break;
                case 2:
                    xPosition = (float)1 / (float)3; break;
            }

            // Create GameObject to contain lane
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "Lane_" + (TrackLane.LaneType)i;
            obj.transform.parent = gameObject.transform;
            obj.transform.localPosition = new Vector3(xPosition, 1, 0);
            obj.transform.localScale = new Vector3(0.333f, 0.01f, 1);
            Destroy(obj.GetComponent<BoxCollider>());
            Destroy(obj.GetComponent<MeshRenderer>());

            if (RhythmicGame.DebugDrawLanes)
            {
                var material = (Material)Resources.Load("Materials/GenericMaterial", typeof(Material));

                obj.GetComponent<MeshRenderer>().material = material;
                obj.GetComponent<MeshRenderer>().material.color = new Color(
                    0, 0, 0, (float)i / (float)3);
            }

            // create and assign Lane
            TrackLane lane = obj.AddComponent<TrackLane>();
            lane.laneType = (TrackLane.LaneType)i;

            // assign newly created lane in Lanes[] array
            Lanes[i] = lane;

            if (RhythmicGame.DebugLaneCreationEvents)
                Debug.LogFormat("TRACK [{0}]: Created new lane: {1}", TrackName, lane.laneType.ToString());
        }
    }

    /// <summary>
    /// Populate lanes with Note (CATCH) objects from the list of MIDI events
    /// </summary>
    public void PopulateLanes()
    {
        // TODO: Rhythmic lane population!

        //if (RhythmicGame.DebugNoteCreationEvents)
        //    Debug.LogFormat(string.Format("TRACK [{0}]: Created new note: {1}", TrackName, noteName));
    }

    private void OnValidate()
    {
        SetTrackMaterialByInstrument(Instrument);
    }

    public static class Colors
    {
        public static Color Invalid = new Color(0, 0, 0);
        public static Color Drums = new Color(212, 93, 180);
        public static Color Bass = new Color(87, 159, 221);
        public static Color Synth = new Color(221, 219, 89);

        public static Color ColorFromTrackType(TrackType type)
        {
            switch (type)
            {
                default:
                    return Invalid;

                case TrackType.Drums:
                    return Drums;
                case TrackType.Bass:
                    return Bass;
                case TrackType.Synth:
                    return Synth;
            }
        }

        public static Color ConvertColor(Color color)
        {
            return new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
        }
    }

    // Track types
    public enum TrackType
    {
        Drums = 0,
        Bass = 1,
        Synth = 2,
        Guitar = 3,
        Vocals = 4,
        FREESTYLE = 5,
        MISC = 6
    }
}
