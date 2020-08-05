using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ChromaticSeparation : MonoBehaviour
{
    private Shader shader;
    private Material material;

    [Range(0.0f, 30.0f)]
    public float Intensity = 1.0f;

    [Range(-1.0f, 1.0f)]
    public float XOffset = 0.5f;

    [Range(-1.0f, 1.0f)]
    public float YOffset = 0.5f;

    //public bool onTheScreenEdges = true;

    public void Start()
    {
        shader = Shader.Find("Hidden/ChromaticSeparation");
        material = new Material(shader);

        if (!shader && !shader.isSupported)
        {
            enabled = false;
        }
    }

    public void OnRenderImage(RenderTexture inTexture, RenderTexture outTexture)
    {
        if (shader != null & Intensity > 0)
        {
            material.SetFloat("_ChromaticSeparation", 0.01f * Intensity);

            /*
            if (onTheScreenEdges)
				material.SetFloat("_Center", 0.5f);

            else
				material.SetFloat("_Center", 0);
			*/

            material.SetFloat("_CenterX", XOffset);
            material.SetFloat("_CenterY", YOffset);

            Graphics.Blit(inTexture, outTexture, material);
        }
        else
        {
            Graphics.Blit(inTexture, outTexture);
        }
    }
}