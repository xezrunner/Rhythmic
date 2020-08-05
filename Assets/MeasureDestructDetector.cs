using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureDestructDetector : MonoBehaviour
{
    Transform ogParent;
    Measure ogMeasure;

    public GameObject Particles;

    void Start()
    {
        ogParent = transform.parent;
        transform.parent = null;

        ogMeasure = ogParent.GetComponent<Measure>();
        ogMeasure.OnCaptureFinished += OgMeasure_OnCaptureFinished;

        transform.localPosition = new Vector3(0, 0, 0f);
        //transform.localScale = new Vector3(2, 1, 0.1f);

        if (ogMeasure.IsMeasureActive)
            Particles.SetActive(true);
    }

    private async void OgMeasure_OnCaptureFinished(object sender, int e)
    {
        await System.Threading.Tasks.Task.Delay(3000);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(ogParent.position.x, ogParent.position.y, ogParent.position.z - 0.05f);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Note")
            other.gameObject.GetComponent<Note>().CaptureNote();
    }
}
