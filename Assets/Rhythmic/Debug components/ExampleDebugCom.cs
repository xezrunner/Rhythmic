using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleDebugCom : DebugCom
{
    public static ExampleDebugCom Instance;

    public override string Main()
    {
        if (!Instance) Instance = this;
        //Write("This is a test - %, %, %", 1, 2, 3);
        return text;
    }

    public void Add(string t)
    {
        text += t + "\n";
        Main();
        DebugSystem.Instance.HandleCurrentComponent();
    }
}
