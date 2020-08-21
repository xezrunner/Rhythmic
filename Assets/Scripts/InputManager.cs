using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEditor.Experimental.GraphView;
using XInputDotNetPure;

public class InputManager
{
    class AxisState
    {
        public AxisState(string name, bool isLocked = false, TextMeshProUGUI debugText = null)
        {
            Name = name;
            IsLocked = isLocked;
            DebugText = debugText;
        }

        public string Name;
        public bool IsLocked;
        public TextMeshProUGUI DebugText;
    }

    public static bool IsController
    {
        get
        {
            if (Input.GetJoystickNames().Length > 0)
                return Input.GetJoystickNames()[0] != "";
            else
                return false;
        }
    }

    static List<AxisState> axisStateList = new List<AxisState>();

    public static string AxisToShortName(string axis)
    {
        switch (axis)
        {
            default:
                return axis;
            case "Left stick":
                return "LS";
            case "Right stick":
                return "RS";
            case "Catch right trigger":
                return "R2";
        }
    }
    public static void ChangeTextColor(TextMeshProUGUI text, Color color)
    { if (text != null) text.color = color; }

    // TODO: mouse will need to be handled with axises - only ignore those controls that are controller-only!
    // TODO: special checks for DS4
    public static bool GetAxisDown(string axisName, float? expectedValue = null, float zero = 0f, float? maxAxis = null, float? minAxis = null)
    {
        if (!IsController)
            return false;

        var axisValue = Input.GetAxisRaw(axisName); // get value for input axis
        if (maxAxis.HasValue || minAxis.HasValue) // clamp value if min or max is given
        {
            /*if (minAxis.HasValue & zero > minAxis.Value)
            {
                Debug.LogWarningFormat("INPUT/GetAxisDown(): zero point override [{0}] was bigger than minimum override [{1}] - returning false!", zero); 
                return false; 
            }*/
            axisValue = Mathf.Clamp(axisValue,
                minAxis.HasValue ? minAxis.Value : Mathf.NegativeInfinity,
                maxAxis.HasValue ? maxAxis.Value : Mathf.Infinity);
        }

        TextMeshProUGUI debugText = null;
        AxisState axisState;

        axisState = axisStateList.Find(x => x.Name == axisName);
        if (axisState == null)
        {
            var debugObj = GameObject.Find(string.Format("{0}_Debug", axisName)); // find debug text object
            if (debugObj != null)
                debugText = debugObj.GetComponent<TextMeshProUGUI>();
            axisState = new AxisState(axisName, false, debugText); axisStateList.Add(axisState);
        }

        // Update debug text if exists
        debugText = axisState.DebugText;
        if (debugText != null)
            debugText.text = string.Format("{0}: {1}", AxisToShortName(axisName), axisValue.ToString());


        if (axisValue == zero) // if the input value is zero, UNLOCK axis
        {
            axisState.IsLocked = false; ChangeTextColor(debugText, Color.white);
            return false;
        }

        if (axisState.IsLocked) // if axis is LOCKED, return false!
            return false;

        // if axis is UNLOCKED:

        // are we beyond the expected value
        bool isInputBeyondExpectedValue =
            !expectedValue.HasValue ? axisValue != zero : // if no expected value is defined, check whether we exceeded zero
            (expectedValue < 0f & axisValue <= expectedValue) || (expectedValue > 0f & axisValue >= expectedValue); // if no direction is specified, check if we met or exceeded expectedValue

        if (isInputBeyondExpectedValue) // LOCK the axis and return true!
        {
            axisState.IsLocked = true; ChangeTextColor(debugText, Color.red);
            return true;
        }
        else // leave axis unlocked, return false!
            return false;
    }
    public static bool GetButtonDown(string inputName)
    {
        if (inputName.Contains("trigger"))
            // Not giving the GetAxisDown() method an expectedValue will make it check whether the input axis is anything other than the zero point.
            // Here, we specify a null expectedValue and override the zero point to the unpressed state of the DS4's R2 button (-1).
            return GetAxisDown(inputName, null, -1f);
        else
            return Input.GetButtonDown(inputName);
    }

    public async static void BeatHaptic(float strength = 60f)
    {
        GamePad.SetVibration(PlayerIndex.One, 1f, 0f);
        await Task.Delay(100);
        GamePad.SetVibration(PlayerIndex.One, 0, 0f);
    }

    public static class Player
    {
        public static string[] TrackSwitchingInputs = new string[] { "Switch left", "Switch right", "D-pad", };
    }

    public static class Catcher
    {
        public static string[] Inputs = new string[] { "Catch left", "Catch center", "Catch right", "Catch right trigger" };

        /// <summary>
        /// Gives back the appropriate key for the lane type.
        /// </summary>
        /// <param name="lane">The lane in question</param>
        public static string TrackLaneToInput(Track.LaneType lane) { return Inputs[(int)lane]; }
        /// <summary>
        /// Gives back the appropriate track for the input key
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        public static Track.LaneType InputToTrackLane(string input) { return (Track.LaneType)(Mathf.Clamp(Array.IndexOf(Inputs, input), 0, 2)); }
    }
}
