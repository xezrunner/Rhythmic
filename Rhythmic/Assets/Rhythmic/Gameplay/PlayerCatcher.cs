using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class PlayerCatcher : MonoBehaviour {

    void Start() {
        if (catchers.Count == 0) {
            int lane_count_div = Variables.TRACK_LaneCount / 2;

            for (int i = -lane_count_div; i < lane_count_div; ++i) {
                Log("id: %", i);
                catchers.Add(Catcher.PREFAB_Create(i, transform));
            }
        }
    }

    public List<Catcher> catchers = new List<Catcher>();

}