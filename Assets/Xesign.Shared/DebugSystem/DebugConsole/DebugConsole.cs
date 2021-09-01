using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DebugCom("Prefabs/DebugSystem/DebugConsole")]
public class DebugConsole : DebugCom
{
    public RectTransform UI_Panel_Trans;

    public RectTransform UI_TextContainer;
    public TMP_InputField UI_InputField;

    public override void Awake()
    {
        base.Awake();
        // UI_Line_Prefab = (GameObject)Resources.Load("Prefabs/DebugSystem/DebugConsoleLine");
    }

    public GameObject UI_Line_Prefab;
    public void UI_AddLine(string text, Color? color = null)
    {
        GameObject obj = Instantiate(UI_Line_Prefab, UI_TextContainer);
        TMP_Text line = obj.GetComponent<TMP_Text>();

        line.SetText(text);
        if (color.HasValue) line.color = color.Value;
    }

    public void UI_ClearLines()
    {
        foreach (RectTransform t in UI_TextContainer)
            Destroy(t);
    }
}
