using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DebugUILevel
{
    None = 0,
    Framerate = 1 << 0,
    DebugLine = 1 << 1,
    ShortStats = 1 << 2,
    ShortShortStats = 1 << 3,
    Stats = 1 << 4,
    InternalStats = 1 << 5,

    Default = (Framerate | DebugLine | ShortStats),
    Full = (Framerate | DebugLine | Stats),
    Internal = (Stats | InternalStats)
}
public enum ComponentDebugLevel
{
    None = 0,
    PlayerStats = 1 << 0,
    SongStats = 1 << 1,
    TracksStats = 1 << 2,
    TracksDetailedStats = 1 << 3,
    AudioStats = 1 << 4,
    ClockStats = 1 << 5,
    LightingStats = 1 << 6,
    AnimationStats = 1 << 7,
    Misc = 1 << 8,

    Default = (SongStats | PlayerStats | TracksStats | ClockStats | Misc),
    Audio = (SongStats | TracksDetailedStats | AudioStats | ClockStats),
    Lighting = (PlayerStats | LightingStats)
}

[DebugComponent(DebugControllerState.DebugUI)]
public class DebugUI : DebugComponent
{
    public static DebugUI Instance;
    public static DebugComponentAttribute Attribute { get { return (DebugComponentAttribute)System.Attribute.GetCustomAttribute(typeof(DebugUI), typeof(DebugComponentAttribute)); } }

    static GameObject _Prefab;
    public static GameObject Prefab { get { if (!_Prefab) _Prefab = (GameObject)Resources.Load($"Prefabs/Debug/DebugUI"); return _Prefab; } }

    SongController SongController { get { return SongController.Instance; } }

    [Header("Content references")]
    public TextMeshProUGUI framerateText;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI debugLineText;

    [Header("Properties")]
    public bool IsDebugUIOn = true;
    public bool IsDebugPrintOn = true;

    void Awake() => _Instance = Instance = this;

    /// Debug line

    int bananasCounter = -1;

    // TODO: improve this! Add Logger compatibility!
    // TODO: Colors!
    public static void AddToDebugLine(string text) => Instance._AddToDebugLine(text);
    void _AddToDebugLine(string text)
    {
        if (debugLineText.text.Length == 0) { debugLineText.text = text; return; }
        string s = debugLineText.text;

        int charCount = 0;
        int newlineCount = 0;
        for (int i = 0; i < s.Length; i++, charCount++)
        {
            if (s[i] == '\n')
                newlineCount++;
        }

        if (charCount == 0) // assume that we have one line without a newline
            charCount = s.Length;

        s = s.Insert(charCount, '\n' + text);
        newlineCount++;

        if (newlineCount >= 4) // line cleanup! max 4 lines!
            s = s.Remove(0, s.IndexOf('\n') + 1);

        debugLineText.text = s;
    }

    /// Main debug text

    void UpdateMainDebugText()
    {
        if (!SongController.IsEnabled)
        {
            debugText.text = "SongController Enabled: False";
            return;
        }

        int trackCount = TracksController.Instance.Tracks.Count;
        string trackNames = "";
        TracksController.Instance.Tracks.ForEach(t => trackNames += $"{t.TrackName}  ");

        string s = $"World: DevScene\n" +
                   $"Room path: /rooms/_u_trans_/dev/dev_scene.drm [SceneToRoom]\n\n" +

                   $"SongController Enabled: {SongController.IsEnabled}\n" +
                   $"Song name: {SongController.songName}\n" +
                   $"Song BPM: {SongController.songBpm}  Song scale: {SongController.songFudgeFactor.ToString("0.00")}\n\n" +

                   $"Tracks: {trackNames}({trackCount})\n\n" +

                   $"SlopMs: {SongController.SlopMs}  SlopPos: {SongController.SlopPos}\n\n" +

                   $"Timscale: [world: {Time.timeScale.ToString("0.00")}]  [song: {SongController.songTimeScale.ToString("0.00")}]\n" +
                   $"Clock seconds: {Clock.Instance.seconds}\n" +
                   $"Clock bar: {(int)Clock.Instance.bar}\n" +
                   $"Clock beat: {(int)Clock.Instance.beat % 8} ({(int)Clock.Instance.beat})\n" +
                   $"Locomotion distance: {AmpPlayerLocomotion.Instance.DistanceTravelled}\n\n" +

                   //$"LightManager: null | LightGroups:  (0)";
                   "";

        if (!IsDebugPrintOn)
            s += "\n\nDEBUG PRINT FREEZE";

        debugText.text = s;
    }

    /// Debug main loop

    private void ProcessKeys()
    {
        // Debug control
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F3))
        {
            IsDebugUIOn = !IsDebugUIOn;
            //Main.SetActive(IsDebugOn);
        }

        if (!IsDebugUIOn)
            return;

        // ----- DEBUG LOOP ----- //

        if (Input.GetKeyDown(KeyCode.L))
            AddToDebugLine($"bananas! {bananasCounter++}");

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F3)) // freeze printing
        {
            IsDebugPrintOn = !IsDebugPrintOn;
            UpdateMainDebugText();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F4)) // disable debug print UI
        {
            IsDebugPrintOn = !IsDebugPrintOn;
            //Main.SetActive(IsDebugPrintOn); // TODO: fix!
        }
    }

    float deltaTime;
    void Update()
    {
        ProcessKeys();

        if (!IsDebugUIOn)
            return;

        // DEBUG LOOP

        if (IsDebugPrintOn)
            UpdateMainDebugText();

        // update framerate debug
        if (Time.timeScale == 0f)
            return;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        framerateText.text = string.Format("Framerate: {0} FPS", Mathf.Ceil(fps).ToString());
    }
}
