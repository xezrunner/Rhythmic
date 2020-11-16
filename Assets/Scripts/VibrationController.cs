using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public static class VibrationController
{
    public class Vibration
    {
        // Unique ID for a vibration.
        public Guid GUID = Guid.NewGuid();

        /// <summary>
        /// x = bigMotor | y = smallMotor | z = duration (ms)
        /// </summary>
        // Vibration target values and duration
        public Vector3 TargetHaptics = new Vector3(0, 0, 0);
    }

    // The current vibration
    static Vibration currentVibration;
    static bool Enabled = false;

    // Vibrates the controller for a specific duration.
    // The motor values get set, then re-set to 0,0 without easing effects.
    // If another vibration comes in, the motors will not be re-set to 0,0, in order to not cancel the new vibration.
    public static async void VibrateLinear(Vector3 haptics)
    {
        if (!Enabled) return;
        if (Gamepad.current == null) // no controller detected
            return;

        Vibration vibr = new Vibration(); // create a new vibration object
        currentVibration = vibr; // set to current vibration

        Gamepad.current.SetMotorSpeeds(haptics.x, haptics.y); // set motor speeds to target haptics

        await Task.Delay(TimeSpan.FromMilliseconds(haptics.z)); // wait for duration

        if (currentVibration == vibr) // reset vibration to 0 if the last vibration is the same
        { Gamepad.current.SetMotorSpeeds(0, 0); currentVibration = null; }
    }
}
