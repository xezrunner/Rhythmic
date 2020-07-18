using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatcherController : MonoBehaviour
{
    public static CatcherController Instance;
    public List<Catcher> Catchers = new List<Catcher>();
    public KeyCode[] keycodes = new KeyCode[] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow };

    float _catcherCollisionRadius;
    public float CatcherCatchRadius
    {
        get { return _catcherCollisionRadius; }
        set
        {
            _catcherCollisionRadius = value;
            foreach (Catcher catcher in Catchers)
                catcher.catchRadius = value;
        }
    }

    void Awake()
    {
        Instance = this;

        // find catchers
        foreach (Transform obj in transform)
        {
            Catcher catcher = obj.GetComponent<Catcher>();
            catcher.OnCatch += Catcher_OnCatch;
            if (catcher != null)
                Catchers.Add(catcher);
        }
    }

    public event EventHandler<CatchEventArgs> OnCatch;

    public class CatchEventArgs
    {
        public Catcher.CatchResult? catchresult;
        public Track.LaneType lane;
        public Note note;
        public Note.NoteType? notetype;
        public Catcher.NoteMissType? noteMissType;
    }

    private void Catcher_OnCatch(object sender, CatchEventArgs e)
    {
        OnCatch?.Invoke(null, e);
    }

    public void PerformCatch(Track.LaneType lane)
    {
        GetCatcherFromLane(lane).PerformCatch();
    }

    KeyCode pressedKey = KeyCode.None;
    bool m_pressingButton = false;
    void Update()
    {
        // INPUT
        foreach (KeyCode key in keycodes)
        {
            if (Input.GetKeyDown(key)) // If the key is down
            {
                if (pressedKey == key) // If it's the same key that has been down before
                {
                    m_pressingButton = true; // We're holding the key
                    continue;
                }
                else // If it's a new key
                {
                    pressedKey = key; // replace the pressed key with the new key
                    m_pressingButton = false; // register as if we only pressed it, but not holding it
                    break;
                }
            }
        }

        if (!m_pressingButton & Input.GetKey(pressedKey)) // if we're not holding the key and the input that's being held is the same as the previous key
        {
            GetCatcherFromKeyCode(pressedKey).PerformCatch();
            m_pressingButton = true; // we're holding the button now - might not be needed because of the above checking? TODO: test!
        }
        if (!Input.GetKey(pressedKey)) // if the prev key is not being pressed, set the prev key to none
            pressedKey = KeyCode.None;
    }

    public Catcher GetCatcherFromKeyCode(KeyCode key)
    {
        return Catchers[(int)KeyCodeToLane(key)];
    }
    public Catcher GetCatcherFromLane(Track.LaneType lane)
    {
        return Catchers[(int)LaneToKeyCode(lane)];
    }
    /// <summary>
    /// Gives back the appropriate key for the lane type.
    /// </summary>
    /// <param name="lane">The lane in question</param>
    public static KeyCode LaneToKeyCode(Track.LaneType lane)
    {
        switch (lane)
        {
            case Track.LaneType.Left:
                return KeyCode.LeftArrow;
            case Track.LaneType.Center:
                return KeyCode.UpArrow;
            case Track.LaneType.Right:
                return KeyCode.RightArrow;

            default:
                return KeyCode.None;
        }
    }
    /// <summary>
    /// Gives back the appropriate track for the input key
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    public static Track.LaneType KeyCodeToLane(KeyCode key)
    {
        switch (key)
        {
            default:
                return Track.LaneType.Center;

            case KeyCode.LeftArrow:
                return Track.LaneType.Left;
            case KeyCode.UpArrow:
                return Track.LaneType.Center;
            case KeyCode.RightArrow:
                return Track.LaneType.Right;
        }
    }
}
