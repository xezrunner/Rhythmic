using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleDebugCom : DebugCom
{
    public static ExampleDebugCom Instance;

    public override string Com_Main()
    {
        if (!Instance) Instance = this;
        //Write("This is a test - %, %, %", 1, 2, 3);
        return com_text;
    }

    public void Add(string t)
    {
        com_text += t + "\n";
        Com_Main();
        DebugSystem.Instance.HandleCurrentComponent();
    }
}
