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
        RhythmicGame.IsLoading = true;

        Debug.LogFormat("GAME [init]: Game type is {0}", RhythmicGame.GameLogic.ToString());

#if UNITY_ANDROID
        RhythmicGame.SetFramerate(60);
#elif UNITY_STANDALONE
        RhythmicGame.SetFramerate(200);
#endif

        ppfx = GameObject.Find("ppfx");

        ppv = ppfx.GetComponent<PostProcessVolume>();
        ppv.profile.TryGetSettings(out dofLayer);
    }

    // Start is called before the first frame update
    async void Start()
    {
        await Task.Delay(100);

#if UNITY_STANDALONE
        if (!SetResolutionOnce & Keyboard.current.leftCtrlKey.isPressed)
            RhythmicGame.SetResolution(RhythmicGame.PreferredResolution); // TODO: this forces you in to exclusive fullscreen mode
        SetResolutionOnce = true;
        Debug.LogFormat("GameStarter: Preferred resolution applied - {0}x{1}.", RhythmicGame.PreferredResolution.x, RhythmicGame.PreferredResolution.y);
#endif

        StartCoroutine(Load("DevScene"));
    }

    IEnumerator Load(string levelName)
    {
        //AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);

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
