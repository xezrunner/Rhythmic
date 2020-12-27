using System.Collections.Generic;
using UnityEngine;

public enum CatcherSide { Left = 0, Center = 1, Right = 2 }
public enum CatchResultType { Success = 0, Empty = 1, Ignore = 2, Miss = 3, Error = 4, UNKNOWN = 5 }

public struct CatchResult
{
    public CatchResult(Catcher c, CatchResultType t, AmpNote n)
    {
        resultType = t;
        catcher = c;
        note = n;
    }

    public CatchResultType resultType;
    public Catcher catcher;
    public AmpNote? note;
}

public class Catcher : MonoBehaviour
{
    SongController SongController;

    [Header("Properties")]
    public int ID;
    public string Name;
    public CatcherSide Side = CatcherSide.Center;

    // ... //
    // Visuals
    // Animations
    // Effects
    // ... //

    void Awake()
    {
        SongController = SongController.Instance;
    }

    private void Start() { }

    public void Catch()
    {

    }
}