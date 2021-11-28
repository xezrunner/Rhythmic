using System;
using UnityEngine;


public class Test2 : MonoBehaviour {
    public GradientColorKey[] colorKey;
    public GradientAlphaKey[] alphaKey;

    public Material mat;

    public Gradient gradient = new Gradient();

    public int angle = 0;

    void Start() {
        colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0f;
        colorKey[1].color = Color.blue;
        colorKey[1].time = 1f;

        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 0.5f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 0.1f;
        alphaKey[1].time = 1.0f;

        Material mat_res = (Material)Resources.Load("Materials/test");
        mat = Instantiate(mat_res);

        gradient.SetKeys(colorKey, alphaKey);
    }

    void Update() {
        //gradient.SetKeys(colorKey, alphaKey);

        Texture2D texture = new Texture2D(1, 128);
        Color[] pixels = new Color[128];
        for (int i = 0; i < 128; i++) {
            pixels[i] = gradient.Evaluate(i / 127f);
        }



        texture.SetPixels(pixels);
        texture.Apply();

        mat.SetTexture("_EmissionMap", texture);
        mat.SetTexture("_MainTex", texture);

        RenderSettings.skybox = mat;
    }


}
