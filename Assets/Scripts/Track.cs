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
    public string TrackName { get { return gameObject.name; } }
    public bool IsTrackActive = true; // TODO: Active and Inactive tracks (Regeneration)
    public TrackLane[] Lanes; // Lanes in this Track

    void Start()
    {
        // Set track material
        SetTrackMaterialByInstrument(Instrument);

        // Create lane splits, if they haven't been created already by TrackController
        //if (Lanes == null)
        CreateLanes();
    }

    // Material

    public void SetTrackMaterial(string materialPath)
    {
        Material finalMat = (Material)Resources.Load(materialPath, typeof(Material));

        if (finalMat != null)
            gameObject.GetComponent<Renderer>().material = finalMat;
        else
            Debug.LogErrorFormat("TRACK [{0}]: Cannot load material {1}", TrackName, materialPath);
    }

    public void SetTrackMaterialByInstrument(TrackType type)
    {
        gameObject.GetComponent<Renderer>().material = GetTrackMaterial(type);
    }

    public string TrackMaterialPath = "Materials/Tracks/";
    Material GetTrackMaterial(TrackType type)
    {
        Material finalMaterial;
        string finalPath = TrackMaterialPath + type.ToString() + "Material";

        try
        {
            finalMaterial = (Material)Resources.Load(finalPath, typeof(Material));
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

    // Notes

    /// <summary>
    /// This creates our Lanes in the Track.
    /// We want to create 3 lanes for Left, Center and Right.
    /// </summary>
    public void CreateLanes()
    {
        Lanes = new TrackLane[3];

        for (int i = 0; i < 3; i++)
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

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "Lane_" + (TrackLane.LaneType)i;
            obj.transform.parent = gameObject.transform;
            obj.transform.localPosition = new Vector3(xPosition, 1, 0);
            obj.transform.localScale = new Vector3(0.333f, 0.01f, 1);
            Destroy(obj.GetComponent<BoxCollider>());
            Destroy(obj.GetComponent<MeshRenderer>());

            /*
            var material = (Material)Resources.Load("Materials/GenericMaterial", typeof(Material));
            obj.GetComponent<MeshRenderer>().material = material;
            obj.GetComponent<MeshRenderer>().material.color = new Color(
                0, 0, 0, (float)i / (float)3);
            */

            // create and assign Lane
            TrackLane lane = obj.AddComponent<TrackLane>();
            lane.trackName = TrackName;
            lane.laneType = (TrackLane.LaneType)i;

            Lanes[i] = lane;

            if (RhythmicGame.DebugLaneCreationEvents)
                Debug.LogFormat("TRACK [{0}]: Created new lane: {1}", TrackName, lane.laneType.ToString());
        }
    }

    /// <summary>
    /// Populate lanes with Note (CATCH) objects from the list of MIDI events
    /// </summary>
    public void PopulateLanes(List<List<MidiEvent>> eventsList)
    {
        /*
        string noteName = "";

        if (RhythmicGame.DebugNoteCreationEvents)
            Debug.LogFormat(string.Format("TRACK [{0}]: Created new note: {1}", TrackName, noteName));
        */
    }

    // Track types

    public enum TrackType
    {
        Bass = 1,
        Drums = 0,
        Synth = 2,
        Guitar = 3,
        Vocals = 4,
        FREESTYLE = 5,
        MISC = 6
    }
}
