using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneAsync : SingletonObject<ChangeSceneAsync>
{
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TMP_Text loadingText;

    // the actual percentage while scene is fully loaded
    private const float LOAD_READY_PERCENTAGE = 0.9f;

    public void ChangeScene(string sceneName)
    {
        loadingUI.SetActive(true);
        //loadingText.text = "LOADING...";
        StartCoroutine(LoadingSceneRealProgress(sceneName));
    }

    private IEnumerator LoadingSceneRealProgress(string sceneName) 
    {
        //yield return new WaitForSecondsRealtime(1f);
        var operation = SceneManager.LoadSceneAsync(sceneName);
        // disable scene activation while loading to prevent auto load
        operation.allowSceneActivation = false;

        while (!operation.isDone) {
            loadingBar.value = operation.progress;
            if (operation.progress >= LOAD_READY_PERCENTAGE) {
                loadingBar.value = 1f;
                //loadingText.text = "PRESS SPACE TO CONTINUE";
                /*if (Input.GetKeyDown(KeyCode.Space)) {
                    operation.allowSceneActivation = true;
                }*/ 
                operation.allowSceneActivation = true;
            }
            Debug.Log(operation.progress);
            yield return null;
        }
    }
}