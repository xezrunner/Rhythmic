using UnityEngine;
using static Logger;

public class Catcher : MonoBehaviour {
    public Transform trans;

    public int id;

    public Catcher Setup(int id, Transform parent) {
        this.id = id;
        trans.SetParent(parent);
        trans.localPosition = new Vector3(Note.GetPosOffsetForLane(id), 0, 0);
        trans.localScale = new Vector3(Variables.NOTE_Size, 0.01f, Variables.NOTE_Size);


        return this;
    }

    // ----- //

    public const string PREFAB_PATH = "Prefabs/" + nameof(Catcher);
    public static GameObject PREFAB_Cache = null;
    public static Catcher PREFAB_Create(int id, Transform parent) {
        if (!PREFAB_Cache) PREFAB_Cache = (GameObject)Resources.Load(PREFAB_PATH);
        if (!PREFAB_Cache && LogE("PREFAB_Cache is null!".TM(nameof(Catcher)))) return null;

        GameObject obj = Instantiate(PREFAB_Cache);

        return obj.GetComponent<Catcher>().Setup(id, parent);
    }
}