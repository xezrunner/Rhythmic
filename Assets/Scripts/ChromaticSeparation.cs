using UnityEngine;

// CHROMATIX PPFX
// Based on https://github.com/brunurd/unity-chromatic-aberration

/// <summary>
/// Name: Chromatic Separation
/// Type: Post-processing effect
/// 
/// Effect: the Blue and Red screen color channels separate to the left / right. Green is the base color channel that stays still.
/// Purpose: This is used in Amplitude as an animated effect when crossing through a checkpoint and during the synesthetic state
///          at the end of the game.
/// </summary>

[ExecuteInEditMode]
public class ChromaticSeparation : MonoBehaviour
{
    private Shader shader;
    private Material material;

    [Range(0.0f, 30.0f)]
    public float Intensity = 0.0f;
    [Range(0f, 1f)]
    public float XOffset = 0.5f;
    [Range(0f, 1f)]
    public float YOffset = 0f;

    public void Start()
    {
        shader = Shader.Find("Hidden/ChromaticSeparation");
        material = new Material(shader);

        if (!shader && !shader.isSupported)
            enabled = false;
    }
    public void OnRenderImage(RenderTexture inTexture, RenderTexture outTexture)
    {
        if (shader != null & Intensity > 0)
        {
            material.SetFloat("_ChromaticSeparation", 0.01f * Intensity);
            material.SetFloat("_CenterX", XOffset);
            material.SetFloat("_CenterY", YOffset);

            Graphics.Blit(inTexture, outTexture, material);
        }
        else
            Graphics.Blit(inTexture, outTexture);
    }
}