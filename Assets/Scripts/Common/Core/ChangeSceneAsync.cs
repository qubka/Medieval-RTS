using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneAsync : SingletonObject<ChangeSceneAsync>
{
    private AsyncOperation loadingOperation;
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TMP_Text loadingText;

    // the actual percentage while scene is fully loaded
    private const float LoadReadyPercentage = 0.9f;

    public void ChangeScene(string sceneName)
    {
        loadingUI.SetActive(true);
        //loadingText.text = "LOADING...";
        StartCoroutine(LoadingSceneRealProgress(sceneName));
    }

    private IEnumerator LoadingSceneRealProgress(string sceneName) 
    {
        //yield return new WaitForSecondsRealtime(1f);
        loadingOperation = SceneManager.LoadSceneAsync(sceneName);
        // disable scene activation while loading to prevent auto load
        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone) {
            loadingBar.value = MathExtention.Clamp01(loadingOperation.progress / LoadReadyPercentage);
            if (Mathf.Approximately(loadingOperation.progress, LoadReadyPercentage)) {
                loadingOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}