using System;
using UnityEngine;

public static class InputManager
{
    public static class Player
    {
        public static KeyCode[] TrackSwitching = new KeyCode[2] { KeyCode.A, KeyCode.D };
    }

    public static class Catcher
    {
        public static KeyCode[] Catching = new KeyCode[3] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow };

        /// <summary>
        /// Gives back the appropriate key for the lane type.
        /// </summary>
        /// <param name="lane">The lane in question</param>
        public static KeyCode TrackLaneToKeyCode(Track.LaneType lane)
        {
            return Catching[(int)lane];
        }
        /// <summary>
        /// Gives back the appropriate track for the input key
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        public static Track.LaneType KeyCodeToTrackLane(KeyCode key)
        {
            return (Track.LaneType)Array.IndexOf(Catching, key);
        }
    }
}
