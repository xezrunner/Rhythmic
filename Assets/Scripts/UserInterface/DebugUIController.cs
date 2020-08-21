using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class DebugUIController : MonoBehaviour
{
    public GameObject section_debug;
    public GameObject section_controllerinput;

    public bool isDebugOn = true;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            isDebugOn = !isDebugOn;
            section_debug.SetActive(isDebugOn);
        }

        if (!isDebugOn)
            return;

        if (section_controllerinput.activeInHierarchy != InputManager.IsController)
            section_controllerinput.SetActive(InputManager.IsController);
    }
}
