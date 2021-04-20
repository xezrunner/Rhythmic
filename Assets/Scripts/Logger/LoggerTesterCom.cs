using System.Collections.Generic;
using System;
using UnityEngine;

public class LoggerTesterCom : MonoBehaviour
{
    [Header("Input")]
    public string Text;
    public CLogType LogType = CLogType.Info;

    public List<string> TestList = new List<string>();

    public void SendText()
    {
        string debugInfo = $"CLogType: {LogType} [{(int)LogType}]";
        Logger.LogWarning("I broke this.");
        //Logger.Log($"{Text} | {debugInfo}", LogType);
    }

    public void SendObject(bool printIndex = true, char separatorChar = ',') => Logger.Log(TestList, 0, printIndex, separatorChar);
}