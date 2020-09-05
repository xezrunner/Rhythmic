using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureDestructionNoteDetector : MonoBehaviour
{
    public float startPos;
    public float endPos;

    public int deltaMeasures;

    void Start()
    {
        // Get measure numbers for start and end pos
        int startMeasure = SongController.Instance.GetMeasureNumForZPos(startPos);
        int endMeasure = SongController.Instance.GetMeasureNumForZPos(endPos);

        deltaMeasures = (endMeasure - startMeasure); // how many measures between start and end
    }

    void Update()
    {
        //transform.position = new Vector3(transform.position.x, transform.position.y, startPos + ((endPos - startPos)));
        transform.Translate(Vector3.forward * (SongController.Instance.subbeatLengthInzPos * 8) / 30);

        if (transform.position.z >= endPos)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Measure")
        {
            Measure measure = other.gameObject.GetComponent<Measure>();
            measure.CaptureMeasure();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Note")
        {
            Note note = other.gameObject.GetComponent<Note>();
            note.CaptureNote();
        }
    }
}
