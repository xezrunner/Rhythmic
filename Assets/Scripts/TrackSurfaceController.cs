using System.Dynamic;
using UnityEngine;

public class TrackSurfaceController : MonoBehaviour
{
    [SerializeField]
    float m_length = 100f;
    [SerializeField]
    float m_inverselength = 0f;

    public GameObject PivotContainer { get { return transform.GetChild(0).gameObject; } }

    public float Length
    {
        get { return m_length; }
        set
        {
            m_length = value;

            ScaleAndPosition();
        }
    }

    public float InverseLength
    {
        get { return m_inverselength; }
        set
        {
            m_inverselength = value;

            ScaleAndPosition();
        }
    }

    void ScaleAndPosition()
    {
        PivotContainer.transform.localScale = new Vector3(PivotContainer.transform.localScale.x, PivotContainer.transform.localScale.y, m_length - m_inverselength);
        PivotContainer.transform.position = new Vector3(PivotContainer.transform.position.x, PivotContainer.transform.position.y, m_inverselength);
    }

    private void OnValidate()
    {
        ScaleAndPosition();
    }
}
