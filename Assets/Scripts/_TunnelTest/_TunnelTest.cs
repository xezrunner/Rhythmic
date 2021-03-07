using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// Testing the tunnel.
public class _TunnelTest : MonoBehaviour
{
    public Tunnel Tunnel;
    public LineRenderer lineRenderer;
    public int Count;
    public int ObjectTargetID;
    public float ObjectRotation;

    public void Start()
    {
        RhythmicGame.IsTunnelMode = true;
        Tunnel.Init(Count);

        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        CreatePoints(Tunnel.radius, Tunnel.radius);
    }
    public GameObject LastObj;
    public List<GameObject> Objects;
    public void AddObject(int id = -1)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        RefreshObject(obj, id);

        LastObj = obj;
        Objects.Add(obj);
    }

    public void RefreshObject() => RefreshObject(LastObj);
    public void RefreshObject(GameObject obj, int id = -1)
    {
        Vector3[] t = Tunnel.GetTransformForTrackID(id == -1 ? ObjectTargetID : id, ObjectRotation);
        obj.transform.position = t[0];
        obj.transform.localScale = new Vector3(RhythmicGame.TrackWidth, 1, 1);
        obj.transform.eulerAngles = t[1];
    }

    public void RemoveAll()
    {
        Objects.ForEach(o => Destroy(o));
        Objects.Clear();
    }

    #region Circle
    int segments = 50;

    void CreatePoints(float xradius, float yradius)
    {
        float x;
        float y;
        float z;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0));

            angle += (360f / segments);
        }
    }
    #endregion
}

