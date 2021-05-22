using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    GameObject ppfx;
    public TextMeshProUGUI loadingText;
    PostProcessVolume ppv;

    public DepthOfField dofLayer = null;

    public static bool SetResolutionOnce = false;

    void Awake()
    {
        GameState.IsLoading = true;

        Debug.LogFormat("GAME [init]: Game type is {0}", RhythmicGame.GameLogic.ToString());

#if UNITY_ANDROID
        RhythmicGame.SetFramerate(60);
#elif UNITY_STANDALONE
        RhythmicGame.SetFramerate(200, 1);
#endif

        GameState.CreateGameState();

        ppfx = GameObject.Find("ppfx");

        ppv = ppfx.GetComponent<PostProcessVolume>();
        ppv.profile.TryGetSettings(out dofLayer);
    }
    // Start is called before the first frame update
    void Start()
    {
        //await Task.Delay(100);

#if UNITY_STANDALONE
        if (!SetResolutionOnce & Keyboard.current.leftCtrlKey.isPressed)
            RhythmicGame.SetResolution(RhythmicGame.PreferredResolution); // TODO: this forces you in to exclusive fullscreen mode
        SetResolutionOnce = true;
        Logger.Log("Preferred resolution applied - %x%".T(this), RhythmicGame.PreferredResolution.x, RhythmicGame.PreferredResolution.y);
#endif

        // Hide cursor
        if (!Application.isEditor) Cursor.visible = false;

        StartCoroutine(Load(RhythmicGame.StartWorld));
        //StartCoroutine(Load("TestScene"));
    }

    IEnumerator Load(string levelName)
    {
        //AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Scenes/" + levelName, LoadSceneMode.Additive);

        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            float progress = asyncOperation.progress * 100f;
            loadingText.text = string.Format("Loading... {0}%", progress.ToString("0"));
            if (asyncOperation.progress >= 0.9f)
                asyncOperation.allowSceneActivation = true;

            yield return null;
        }

        SceneManager.UnloadSceneAsync("Loading");
    }

    void FixedUpdate()
    {
        dofLayer.focusDistance.value += 0.06f;
    }
}
