using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class DebugUIController : MonoBehaviour
{
    GameObject textGO;
    RectTransform uitrans;

    void Start()
    {
        textGO = new GameObject();
        textGO.transform.SetParent(gameObject.transform, false); // parent to Canvas
        uitrans = textGO.AddComponent<RectTransform>();

        //CreateText("wow");
    }

    private void OnGUI()
    {
        /*
        // Position text GameObject
        Vector2 GOpivot = new Vector2(0, 1); // top-left
        uitrans.anchorMin = GOpivot; uitrans.anchorMax = GOpivot;
        uitrans.pivot = GOpivot;
        uitrans.anchoredPosition3D = new Vector3(0, 0);
        uitrans.anchoredPosition = new Vector2(0, 0);
        */
    }

    public void CreateText(string Text, string Name = null)
    {
        Text text = textGO.AddComponent<Text>();
        text.name = Name;
        text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.text = Text;

        RectTransform texttrans = text.GetComponent<RectTransform>();
        //texttrans.position = new Vector3(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
