using UnityEngine;

public class Player : MonoBehaviour {
    public static Player SINGLEPLAYER_Instance;

    public PlayerLocomotion locomotion;

    public new string name;

    void Awake() {
        // if (is singleplayer game)
        SINGLEPLAYER_Instance = this;
        name = gameObject.name;
    }
}