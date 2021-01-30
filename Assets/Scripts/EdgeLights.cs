using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class EdgeLights : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;

    public Mesh Mesh { get { return MeshFilter.mesh; } set { MeshFilter.mesh = value; } }

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

            foreach (Material mat in MeshRenderer.sharedMaterials)
            {
                mat.SetColor("_Color", _convertedColor);
                mat.SetColor("_Emission", _convertedColor * _glowIntensity);
            }
        }
    }

    float _glowIntensity = 2.5f;
    public float GlowIntenstiy
    {
        get { return _glowIntensity; }
        set 
        {
            _glowIntensity = value;

            foreach (Material mat in MeshRenderer.sharedMaterials)
                mat.SetColor("_EmissionColor", _convertedColor * value);
        }
    }
}
