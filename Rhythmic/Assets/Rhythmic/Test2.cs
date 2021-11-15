using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public PathTransform path_trans;

    public float a = 0f;
    public float speed = 0.5f;

    void Update()
    {
        a += speed * Time.deltaTime;
        if (a >= 1f || a <= 0) speed = -speed;

        path_trans.ChangeClipValues(a, -1);
    }
}