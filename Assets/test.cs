using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class test : MonoBehaviour
{
    async void Start()
    {
        await Task.Delay(5000);
        Destroy(gameObject);
    }
}
