using UnityEngine;
using System.Collections;

public class Catcher : MonoBehaviour
{
    AudioSource src { get { return GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>(); } }

    public AudioClip catcher_empty;
    public AudioClip catcher_miss;
    public AudioClip streak_lose;

    public TrackLane.LaneType laneType;

    KeyCode[] keycodes;
    KeyCode[] keycodes_left;
    KeyCode[] keycodes_center;
    KeyCode[] keycodes_right;
    private void Start()
    {
        catcher_empty = (AudioClip)Resources.Load("Sounds/catcher_empty");
        catcher_miss = (AudioClip)Resources.Load("Sounds/catcher_miss");
        streak_lose = (AudioClip)Resources.Load("Sounds/streak_lose");

        keycodes = new KeyCode[] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow };
        keycodes_left = new KeyCode[] { KeyCode.LeftArrow, };
        keycodes_center = new KeyCode[] { KeyCode.UpArrow, };
        keycodes_right = new KeyCode[] { KeyCode.RightArrow };
    }

    private void Update()
    {
        /*
        if (m_pressingButton == false)
        {
            foreach (KeyCode key in keycodes)
            {
                if (Input.GetKeyDown(key))
                {
                    pressedKey = key;
                    break;
                }
            }
        }

        if (pressedKey != KeyCode.None & !Input.GetKey(pressedKey))
        {
            m_pressingButton = false;
            pressedKey = KeyCode.None;
        }

        if (!m_pressingButton & pressedKey != KeyCode.None)
        {
            RaycastHit hitPoint;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hitPoint, 150f))
            {
                Note note = hitPoint.collider.gameObject.GetComponent<Note>();
                if (pressedKey == LaneToKey(note.noteLane))
                    Destroy(hitPoint.collider.gameObject);
                else
                    src.PlayOneShot(catcher_miss);
            }
            else
                src.PlayOneShot(catcher_empty);

            m_pressingButton = true;
        }
        */
    }

    public bool PerformCatch(KeyCode pressedKey)
    {
        RaycastHit hitPoint;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hitPoint, 150f))
        {
            Note note = hitPoint.collider.gameObject.GetComponent<Note>();
            if (pressedKey == LaneToKey(note.noteLane))
            {
                Destroy(hitPoint.collider.gameObject);
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }

    KeyCode LaneToKey(TrackLane.LaneType lane)
    {
        switch (lane)
        {
            case TrackLane.LaneType.Left:
                return KeyCode.LeftArrow;
            case TrackLane.LaneType.Center:
                return KeyCode.UpArrow;
            case TrackLane.LaneType.Right:
                return KeyCode.RightArrow;

            default:
                return KeyCode.None;
        }
    }
}
