using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackEdgeLightController : MonoBehaviour
{
    public GameObject[] EdgeObjects
    {
        get
        {
            GameObject[] edgeObjs = new GameObject[2]; 
            edgeObjs[0] = transform.GetChild(1).transform.GetChild(0).gameObject;
            edgeObjs[1] = transform.GetChild(1).transform.GetChild(1).gameObject;
            return edgeObjs;
        } 
    }
    public Color Color;

    private void Start()
    {
        ApplyMaterial();
    }

    public void ApplyMaterial()
    {
        var material = (Material)Resources.Load("Materials/EdgeLightMaterial");

        material.color = Color;
        material.SetColor("_EmissionColor", Color);

        foreach (GameObject obj in EdgeObjects) 
            obj.GetComponent<MeshRenderer>().material = material;
    }

    private void OnValidate()
    {
        ApplyMaterial();
    }
}
