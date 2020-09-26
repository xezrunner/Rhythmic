using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EdgeLights : MonoBehaviour
{
    MeshRenderer meshRenderer { get { return gameObject.GetComponent<MeshRenderer>(); } }

    bool _isActive;
    public bool IsActive
    {
        get { return _isActive; }
        set { _isActive = value; gameObject.SetActive(value); }
    }

    Color _color = Color.white;
    Color _convertedColor = Color.white;
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;

            // Convert color to 0-1 value, if required
            bool requiresConversion = value.r > 1 || value.g > 1 || value.b > 1;
            if (requiresConversion)
                _convertedColor = Colors.ConvertColor(value);
            else
                _convertedColor = value;

            foreach (Material mat in meshRenderer.materials)
            {
                mat.color = _convertedColor;
                mat.SetColor("_EmissionColor", _convertedColor * _glowIntensity);
            }
        }
    }

    float _glowIntensity = 2f;
    public float GlowIntenstiy
    {
        get { return _glowIntensity; }
        set 
        {
            _glowIntensity = value;

            foreach (Material mat in meshRenderer.materials)
                mat.SetColor("_EmissionColor", _convertedColor * value);
        }
    }
}
