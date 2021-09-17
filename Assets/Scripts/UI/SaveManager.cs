using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : SingletonObject<SaveManager>
{
    [SerializeField] private RectTransform saveCanvas;
    [SerializeField] private GameObject saveLayout;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingSlider;
    
    private readonly List<GameObject> saves = new List<GameObject>();
    
    private void OnEnable()
    {
        // Only get files that are text files only as you want only .txt 
        foreach (var file in SaveLoadManager.GetFiles()) {
            var obj = Instantiate(saveLayout, saveCanvas);
            var layout = obj.GetComponent<SaveLayout>();
            layout.label.text = Path.GetFileNameWithoutExtension(file);
            layout.date.text = File.GetLastWriteTime(file).ToString("dd MMM, yyyy HH:mm", CultureInfo.InvariantCulture);
            saves.Add(obj);
        }
    }

    private void OnDisable()
    {
        foreach (var save in saves) {
            Destroy(save);
        }
    }
}