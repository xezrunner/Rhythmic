using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureSubBeat : MonoBehaviour
{
    public GameObject SubbeatTrigger;
    public GameObject MeasureTrigger;
    public EdgeLightsController EdgeLights;

    bool _isLastSubbeat;
    public bool IsMeasureSubbeat
    {
        get { return _isLastSubbeat; }
        set
        {
            _isLastSubbeat = value;
            if (value)
                MeasureTrigger.SetActive(true);
            else
                MeasureTrigger.SetActive(false);
        }
    }

    public int subbeatNum;

    public float StartZPos;
    public float EndZPos;

    Color _edgeLightsColor;
    public Color EdgeLightsColor
    {
        get { return _edgeLightsColor; }
        set
        {
            _edgeLightsColor = value;
            EdgeLights.Color = value;
        }
    }

    float _edgeLightsGlowIntensity;
    public float EdgeLightsGlowIntensity
    {
        get { return EdgeLights.GlowIntensity; }
        set 
        {
            EdgeLights.GlowIntensity = value;
        }
    }

    bool _edgeLightsGlow;
    public bool EdgeLightsGlow
    {
        get { return _edgeLightsGlow; }
        set
        {
            _edgeLightsGlow = value;
                EdgeLights.EnableGlow = value;
        }
    }

    private void Awake()
    {
        EdgeLights = transform.GetChild(0).GetChild(1).gameObject.GetComponent<EdgeLightsController>();
    }
}
