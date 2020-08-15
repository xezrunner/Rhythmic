using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureDestructDetector : MonoBehaviour
{
    public Transform ogParent;
    public Measure measure;

    public GameObject Particles;

    void Start()
    {
        // store parent and measure
        /*
        ogParent = transform.parent;
        measure = ogParent.GetComponent<Measure>();
        */

        gameObject.SetActive(false);
    }

    bool isEnabled = false;
    public void Begin()
    {
        if (!measure.IsMeasureActive)
            return;

        // setup detector destroy timeout
        measure.OnCaptureFinished += Measure_OnCaptureFinished;

        gameObject.SetActive(true);

        // un-parent!
        transform.parent = null;

        // set location of destructor to 0,0,0 LOCAL
        transform.localPosition = new Vector3(0, 0, 0f);

        // if the measure this detector is responsible for is not empty, we want particles
        if (!measure.IsMeasureEmptyOrCapturedFull)
            Particles.SetActive(true);
        else
            Destroy(Particles); // Particles are usually destroyed when they finish. If we don't start the particles, we have to destroy them.

        isEnabled = true;
    }

    // Destroy detector but keep particles after measure is finished with capturing
    private void Measure_OnCaptureFinished(object sender, int e)
    {
        if (Particles != null)
        {
            // un-parent particles
            Particles.transform.parent = null;
            Particles.transform.localScale = new Vector3(1, 1, 1);
        }

        measure.OnCaptureFinished -= Measure_OnCaptureFinished; // unsubscribe from the OnCaptureFinished event
        transform.parent = measure.transform; gameObject.SetActive(false);
        //Destroy(gameObject);
    }

    // Update position of detector while it's changing Z scale
    void FixedUpdate()
    {
        if (isEnabled)
            transform.position = new Vector3(ogParent.position.x, ogParent.position.y, ogParent.position.z - 0.05f);
    }

    // Capture notes that we encounter throughout the way
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Note")
            other.gameObject.GetComponent<Note>().CaptureNote();
    }
}
