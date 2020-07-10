using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackEdgeLightController : MonoBehaviour
{
    public Transform[] EdgeObjects
    {
        get
        {
            Transform[] edgeObjs = new Transform[2];
            if (transform.name != "TrackPrefab")
            {
                edgeObjs[0] = transform.GetChild(0).transform.GetChild(0);
                edgeObjs[1] = transform.GetChild(0).transform.GetChild(1);
            }
            return edgeObjs;
        }
    }
    public Color Color;

    private void Start()
    {
        ApplyMaterial();
    }

    private bool m_isTrackFocused;
    public bool IsTrackFocused
    {
        get { return m_isTrackFocused; }
        set
        {
            m_isTrackFocused = value;
            ApplyMaterial();

            if (!value & !IsTrackActive)
            {
                EdgeObjects[0].gameObject.SetActive(false);
                EdgeObjects[1].gameObject.SetActive(false);
            }
            else
            {
                EdgeObjects[0].gameObject.SetActive(true);
                EdgeObjects[1].gameObject.SetActive(true);
            }
        }
    }

    private bool m_isTrackActive;
    public bool IsTrackActive { get; set; } = true;
    /*
    {
        get { return m_isTrackActive; }
        set
        {
            m_isTrackActive = value;

            EdgeObjects[0].gameObject.SetActive(value);
            EdgeObjects[1].gameObject.SetActive(value);
        }
    }
    */

    public void ApplyMaterial()
    {
        if (transform.name != "TrackPrefab")
        {
            var material = new Material(Shader.Find("Standard"));

            material.EnableKeyword("_EMISSION");
            material.color = Color;
            material.SetColor("_EmissionColor", Color * (IsTrackFocused ? 1.2f : 0.3f));

            EdgeObjects[0].GetComponent<MeshRenderer>().sharedMaterial = material;
            EdgeObjects[1].GetComponent<MeshRenderer>().sharedMaterial = material;
        }
    }

    private void OnValidate()
    {
        ApplyMaterial();
    }
}
