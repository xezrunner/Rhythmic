using UnityEngine;

using static Logging;

public class TrackController : MonoBehaviour {
    public void init_from_song_info(song_info info) {
        log("init for song '%'".interp(info.name));
    }
}