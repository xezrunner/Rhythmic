using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Logger;

public class TESTLoadingIndicator : MonoBehaviour
{
    public Image Mask;
    public Image Arc;
    
    public float Speed = 0.25f;
    public float RotSpeed = 150f;

    public float Value = 0f;
    public float Mask_Value = 0f;

    void Start()
    {
        StartCoroutine(Rot_Coroutine());
        StartCoroutine(Arc_Coroutine());
    }

    IEnumerator Rot_Coroutine()
    {
        while (true)
        {
            Mask.transform.Rotate((Vector3.back * RotSpeed) * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator Arc_Coroutine()
    {
        while (true)
        {
            Value = 0f; Mask_Value = 1f; Mask.fillAmount = 1f;
            
            float t = 0f;
            
            while (t < 1f)
            {
                Value = Mathf.Lerp(0, 0.7f, t);
                
                Arc.fillAmount = Value;
                //Mask.fillAmount = Value;
                
                t += Speed * Time.deltaTime;
                yield return null;
            }
            
            Log("Arc done!");
            yield return new WaitForSeconds(1f);
            
            t = 0f;
            float og_mask_value = Mask.fillAmount;
            Mask_Value = og_mask_value;
            while (t < 1f)
            {
                Mask_Value = Mathf.Lerp(og_mask_value, (1f - Value) + 0.02f, t);
                Mask.fillAmount = Mask_Value;
                
                t += Speed * Time.deltaTime;
                yield return null;
            }
            
            Log("Mask done!");
            yield return new WaitForSeconds(1f);
        }
    }
}
