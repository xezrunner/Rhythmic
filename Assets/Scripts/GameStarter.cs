using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    GameObject ppfx;
    public TextMeshProUGUI loadingText;
    PostProcessVolume ppv;

    public DepthOfField dofLayer = null;

    void Awake()
    {
        RhythmicGame.IsLoading = true;

        ppfx = GameObject.Find("ppfx");

        ppv = ppfx.GetComponent<PostProcessVolume>();
        ppv.profile.TryGetSettings(out dofLayer);
    }

    // Start is called before the first frame update
    async void Start()
    {
        await Task.Delay(100);

        //if (Screen.fullScreenMode != FullScreenMode.ExclusiveFullScreen)
        Screen.SetResolution(1366, 768, FullScreenMode.ExclusiveFullScreen);

        //await Task.Delay(3000);

        /*
        SceneManager.LoadSceneAsync("DevScene", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync("SnowMountains", LoadSceneMode.Additive);
        */

        StartCoroutine(Load("DevScene"));
    }

    IEnumerator Load(string levelName)
    {
        //AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        Debug.Log("Loading progress: " + asyncOperation.progress);//should get 0 here, right?

        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            float progress = asyncOperation.progress * 100f;
            loadingText.text = string.Format("Loading... {0}%", progress.ToString("0"));
            if (asyncOperation.progress >= 0.9f)
                asyncOperation.allowSceneActivation = true;

            yield return null;
        }
    }

    void FixedUpdate()
    {
        dofLayer.focusDistance.value += 0.06f;
    }
}
