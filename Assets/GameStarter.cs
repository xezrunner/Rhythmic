using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    GameObject ppfx;
    TMPro.TextMeshProUGUI loadingText;
    PostProcessVolume ppv;

    DepthOfField dofLayer = null;

    // Start is called before the first frame update
    async void Start()
    {
        ppfx = GameObject.Find("ppfx");
        loadingText = GameObject.Find("loadingText").GetComponent<TMPro.TextMeshProUGUI>();

        ppv = ppfx.GetComponent<PostProcessVolume>();
        ppv.profile.TryGetSettings(out dofLayer);

        await Task.Delay(3000);
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
        AsyncOperation asyncOperation = Application.LoadLevelAdditiveAsync(levelName);
        Debug.Log("Loading progress: " + asyncOperation.progress);//should get 0 here, right?

        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = "Loading... " + (asyncOperation.progress * 100) + "%";
            if (asyncOperation.progress >= 0.9f)
                asyncOperation.allowSceneActivation = true;

            yield return null;
        }

        loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = "Charting song... ";
    }

    // Update is called once per frame
    void Update()
    {
        dofLayer.focusDistance.value += Time.time * 0.05f;
    }
}
