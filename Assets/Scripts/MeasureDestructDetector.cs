using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureDestructDetector : MonoBehaviour
{
    Transform ogParent;
    Measure measure;

    public GameObject Particles;

    void Start()
    {
        // store parent and measure
        ogParent = transform.parent;
        measure = ogParent.GetComponent<Measure>();

        // un-parent!
        transform.parent = null;

        // setup detector destroy timeout
        measure.OnCaptureFinished += Measure_OnCaptureFinished;

        // set location of destructor to 0,0,0 LOCAL
        transform.localPosition = new Vector3(0, 0, 0f);

        // if the measure this detector is responsible for is not empty, we want particles
        if (!measure.IsMeasureEmpty & measure.IsMeasureActive)
            Particles.SetActive(true);
    }

    // Destroy detector but keep particles after measure is finished with capturing
    private void Measure_OnCaptureFinished(object sender, int e)
    {
        // un-parent particles
        Particles.transform.parent = null;
        Particles.transform.localScale = new Vector3(1, 1, 1);

        measure.OnCaptureFinished -= Measure_OnCaptureFinished; // unsubscribe from the OnCaptureFinished event
        Destroy(gameObject);
    }

    // Update position of detector while it's changing Z scale
    void FixedUpdate()
    {
        transform.position = new Vector3(ogParent.position.x, ogParent.position.y, ogParent.position.z - 0.05f);
    }

    // Capture notes that we encounter throughout the way
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Note")
            other.gameObject.GetComponent<Note>().CaptureNote();
    }
}
