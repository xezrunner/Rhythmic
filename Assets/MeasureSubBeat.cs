using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureSubBeat : MonoBehaviour
{
    Color _edgeLightsColor;
    public Color EdgeLightsColor
    {
        get { return _edgeLightsColor; }
        set
        {
            _edgeLightsColor = value;
            transform.Find("EdgeLights").GetComponent<EdgeLightsController>().Color = value;
        }
    }

    float _edgeLightsGlowIntensity;
    public float EdgeLightsGlowIntensity
    {
        get { return transform.Find("EdgeLights").GetComponent<EdgeLightsController>().GlowIntensity; }
        set 
        {
            transform.Find("EdgeLights").GetComponent<EdgeLightsController>().GlowIntensity = value;
        }
    }

    bool _edgeLightsGlow;
    public bool EdgeLightsGlow
    {
        get { return _edgeLightsGlow; }
        set
        {
            _edgeLightsGlow = value;
                transform.Find("EdgeLights").GetComponent<EdgeLightsController>().EnableGlow = value;
        }
    }
}
