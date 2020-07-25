using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureSubBeat : MonoBehaviour
{
    public GameObject SubbeatTrigger;
    public GameObject MeasureTrigger;

    bool _isLastSubbeat;
    public bool IsMeasureSubbeat
    {
        get { return _isLastSubbeat; }
        set
        {
            _isLastSubbeat = value;
            if (value)
                MeasureTrigger.SetActive(true);
            else
                MeasureTrigger.SetActive(false);
        }
    }

    public int subbeatNum;

    public float StartZPos;
    public float EndZPos;
}
