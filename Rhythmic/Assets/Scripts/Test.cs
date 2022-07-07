using TMPro;
using UnityEngine;

using static Logging;
public class Test : MonoBehaviour {
    void Start() {
        log("This is a % testing whether % is working.".interp("test", "string interpolation"));
    }
}