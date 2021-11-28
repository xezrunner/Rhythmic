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

    public void Catch(int catcher_id) {
        // ..

        catchers[catcher_id]?.Catch();
    }

    void Update() {
        if (Keyboard.current != null) {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) Catch(0);
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) Catch(1);
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) Catch(2);
        }
    }
}