using System;
using UnityEngine;

public class DebugComAttribute : Attribute
{
    public DebugComAttribute() { }
    public DebugComAttribute(float update_freq) { this.update_freq = update_freq; }
    public DebugComAttribute(string prefab_path, float update_freq = -1)
    {
        this.prefab_path = prefab_path;
        this.update_freq = update_freq;
    }

    public bool is_prefab { get { return !prefab_path.IsEmpty(); } }
    public string prefab_path = null;
    public float update_freq = -1;
}

public class DebugCom : MonoBehaviour
{
    public DebugComAttribute Attribute;
    [HideInInspector] public Transform Prefab_Parent;

    public virtual void Awake()
    {
        Attribute = (DebugComAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(DebugComAttribute));
        if (Attribute == null) Attribute = new DebugComAttribute();
    }
    public void Com_Destroy()
    {
        if (Attribute.is_prefab) Destroy(Prefab_Parent);
        else Destroy(this);
    }

    [HideInInspector] public string com_id; // TODO: Performance!
    [HideInInspector] public string com_text;

    public void Com_Clear() => com_text = "";
    public void Com_Write(string t, params object[] args) => com_text += t.Parse(args); // TODO: Performance?
    // public void WriteLn(string t) => Text += t + "\n";

    /// This function is called when the component is active and requested.
    /// It is also called on every update frequency tick.
    public virtual string Com_Main() { return com_text; }
}