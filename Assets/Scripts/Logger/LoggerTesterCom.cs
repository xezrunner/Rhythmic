using System.Collections.Generic;
using System;
using UnityEngine;

public class LoggerTesterCom : MonoBehaviour
{
    [Header("Input")]
    public string Text;
    public CLogType MessageTypeID = CLogType.Application;

    public List<string> TestList = new List<string>();

    public void SendText()
    {
        string debugInfo = $"CLogType: {MessageTypeID} [{MessageTypeID}]";
        Logger.Log($"{Text} | {debugInfo}", MessageTypeID);
    }

    public void SendObject(bool printIndex = true, char separatorChar = ',') => Logger.Log(TestList, 0, printIndex, separatorChar);
}