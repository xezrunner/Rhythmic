using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeLightsController : MonoBehaviour
{
    public List<MeshRenderer> EdgeLightsMeshRenderers
    {
        get
        {
            List<MeshRenderer> finalList = new List<MeshRenderer>();
            foreach (Transform obj in gameObject.transform)
                finalList.Add(obj.GetComponent<MeshRenderer>());
            return finalList;
        }
    }

    Color _color;
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;

            Material Material = new Material(Shader.Find("Standard"));

            Material.color = ConvertColor(value);
            if (EnableGlow)
                Material.EnableKeyword("_EMISSION");
            Material.SetColor("_EmissionColor", ConvertColor(value) * GlowIntensity);

            foreach (MeshRenderer renderer in EdgeLightsMeshRenderers)
                renderer.material = Material;
        }
    }

    bool _isActive = true;
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            gameObject.SetActive(value);
        }
    }

    float _glowIntensity = 1f;
    public float GlowIntensity
    {
        get { return _glowIntensity; }
        set
        {
            _glowIntensity = value;
            if (EnableGlow)
                Color = Color;
        }
    }

    bool _enableGlow = true;
    public bool EnableGlow
    {
        get { return _enableGlow; }
        set
        {
            _enableGlow = value;

            foreach (MeshRenderer renderer in EdgeLightsMeshRenderers)
            {
                if (value)
                    renderer.material.EnableKeyword("_EMISSION");
                else
                    renderer.material.DisableKeyword("_EMISSION");
            }
        }
    }

    public static Color ConvertColor(Color color)
    {
        return new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
    }
}
