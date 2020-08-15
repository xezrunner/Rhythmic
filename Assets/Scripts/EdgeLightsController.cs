using UnityEngine;
using System.Collections.Generic;

// Controls (usually) 2 or more edge lights on a track.

public class EdgeLightsController : MonoBehaviour
{
    // Get a list of all EdgeLight MeshRenderers
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

    // Controls the enabled state of all EdgeLights
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

    Color _color;
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;

            Material Material = EdgeLightsMeshRenderers[0].material;

            Material.color = Colors.ConvertColor(value);
            Material.SetColor("_EmissionColor", Colors.ConvertColor(value) * GlowIntensity);

            if (EnableGlow)
                Material.EnableKeyword("_EMISSION");

            foreach (MeshRenderer renderer in EdgeLightsMeshRenderers)
                renderer.material = Material;
        }
    }

    float _glowIntensity = 2f;
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
}
