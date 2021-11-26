using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Logger;

// TODO: Naming?
public class PlayerCatcherManager : MonoBehaviour {
    public Player player;

    void Start() {
        if (!player) player = Player.SINGLEPLAYER_Instance;

        if (catchers.Count == 0) {
            for (int i = 0; i < Variables.TRACK_Lanes; ++i) {
                catchers.Add(PlayerCatcher.PREFAB_Create(i, transform));
            }
        } else if (catchers.Count != Variables.TRACK_Lanes)
            LogW("Warning: catcher count does not match config for player %.", player.name);
    }

    public List<PlayerCatcher> catchers = new List<PlayerCatcher>();

    void Update() {
        if (Keyboard.current != null) {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) catchers[0].Catch();
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) catchers[1].Catch();
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) catchers[2].Catch();
        }
    }
}